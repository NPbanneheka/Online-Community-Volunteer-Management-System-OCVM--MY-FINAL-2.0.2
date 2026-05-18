// Main event workflow controller: listing, creating, editing, deleting, joining, cancelling, and rating events.
// Technology map:
// - ASP.NET Core MVC actions handle browser requests and form submissions.
// - EF Core LINQ queries VolunteerEvents, EventRegistrations, UserProfiles, Ratings, and Notifications.
// - Identity provides the current logged-in user for ownership and role checks.
// - IWebHostEnvironment is used when event image files are saved under wwwroot.
// Connected files: VolunteerEvent, EventRegistration, UserProfile, UserRating, Notification models and Events views.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;

namespace OCVMS.Controllers;

[Authorize]
public class EventsController : Controller
{
    private const string ApplicationRatingTargetId = "__APPLICATION__"; // Special rating target used when rating the whole platform instead of a single event.
    private const int ApplicationRatingEventId = 0; // EventId placeholder for platform-level ratings.

    private readonly ApplicationDbContext _context; // EF Core context connected to SQL Server tables.
    private readonly UserManager<IdentityUser> _userManager; // Identity service for user lookup, roles, and account operations.
    private readonly IWebHostEnvironment _environment; // Provides wwwroot paths for uploaded image files.

    public EventsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
    }

    [AllowAnonymous]
    // Main listing page: loads records, applies filters/search, prepares ViewBag data, then returns the view.
    public async Task<IActionResult> Index(string? searchTerm, string? statusFilter, DateTime? eventDate)
    {
        await AutoCloseExpiredEventsAsync();

        var eventsQuery = _context.VolunteerEvents
            .Include(e => e.OrganizerProfile)
            .Include(e => e.Registrations)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            eventsQuery = eventsQuery.Where(e =>
                e.Title.Contains(term) ||
                e.Location.Contains(term) ||
                (e.OrganizerProfile != null && e.OrganizerProfile.FullName.Contains(term)) ||
                (e.OrganizerProfile != null && e.OrganizerProfile.OrganizationName != null && e.OrganizerProfile.OrganizationName.Contains(term)));
        }

        if (eventDate.HasValue)
        {
            var selectedDate = eventDate.Value.Date;
            eventsQuery = eventsQuery.Where(e => e.EventDate.Date == selectedDate);
        }

        var today = DateTime.Today;
        statusFilter = string.IsNullOrWhiteSpace(statusFilter) ? "Active" : statusFilter;
        if (statusFilter == "Active")
        {
            eventsQuery = eventsQuery.Where(e =>
                e.EventDate >= today &&
                e.RegistrationOpenDate <= today &&
                (!e.RegistrationClosingDate.HasValue || e.RegistrationClosingDate.Value.Date >= today) &&
                e.Status != "Closed" && e.Status != "Completed" && e.Status != "Cancelled");
        }
        else if (statusFilter == "Upcoming")
        {
            eventsQuery = eventsQuery.Where(e =>
                e.EventDate >= today &&
                e.RegistrationOpenDate > today &&
                e.Status != "Closed" && e.Status != "Completed" && e.Status != "Cancelled");
        }
        else if (statusFilter == "Closed")
        {
            eventsQuery = eventsQuery.Where(e =>
                e.Status == "Closed" ||
                e.Status == "Completed" ||
                e.Status == "Cancelled" ||
                e.EventDate < today ||
                (e.RegistrationClosingDate.HasValue && e.RegistrationClosingDate.Value.Date < today));
        }

        var eventsList = await eventsQuery
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.EventTime)
            .ToListAsync();

        var currentProfile = await GetCurrentProfileAsync();
        var isAdmin = User.IsInRole("Admin");
        ViewBag.CurrentProfileId = currentProfile?.Id;
        ViewBag.CurrentOrganizationName = currentProfile?.OrganizationName;
        ViewBag.IsAdmin = isAdmin;
        ViewBag.ManageableEventIds = eventsList
            .Where(e => CanManageEvent(e, currentProfile, isAdmin))
            .Select(e => e.Id)
            .ToList();

        var userId = _userManager.GetUserId(User);
        ViewBag.UserRegisteredEvents = userId == null
            ? new List<int>()
            : await _context.EventRegistrations
                .Where(r => r.UserId == userId)
                .Select(r => r.VolunteerEventId)
                .ToListAsync();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.StatusFilter = statusFilter;
        ViewBag.EventDate = eventDate?.ToString("yyyy-MM-dd");
        return View(eventsList);
    }

    // Loads events joined by the current volunteer user.

    public async Task<IActionResult> MyEvents()
    {
        await AutoCloseExpiredEventsAsync();

        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        var myRegisteredEvents = await _context.EventRegistrations
            .Where(r => r.UserId == userId)
            .Include(r => r.VolunteerEvent)
                .ThenInclude(e => e!.OrganizerProfile)
            .Select(r => r.VolunteerEvent!)
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.EventTime)
            .ToListAsync();

        return View(myRegisteredEvents);
    }

    [Authorize(Roles = "Organizer,Admin")]
    // Loads events that the current organizer/admin is allowed to manage.
    public async Task<IActionResult> MyCreatedEvents()
    {
        await AutoCloseExpiredEventsAsync();

        var profile = await GetCurrentProfileAsync();
        if (profile == null)
        {
        TempData["Message"] = "Please complete your profile before managing events.";
            return RedirectToAction("Edit", "Profile");
        }

        var isAdmin = User.IsInRole("Admin");
        var events = await _context.VolunteerEvents
            .Include(e => e.OrganizerProfile)
            .Include(e => e.Registrations)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        events = events
            .Where(e => CanManageEvent(e, profile, isAdmin))
            .ToList();

        ViewBag.CurrentProfileId = profile.Id;
        ViewBag.CurrentOrganizationName = profile.OrganizationName;
        ViewBag.IsAdmin = isAdmin;

        return View(events);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Registration action: creates a new user/account or registers the current user for an event depending on controller context.
    public async Task<IActionResult> Register(int eventId)
    {
        await AutoCloseExpiredEventsAsync();

        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        var volunteerEvent = await _context.VolunteerEvents
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (volunteerEvent == null) return NotFound();

        if (!volunteerEvent.IsRegistrationOpen)
        {
        TempData["Message"] = "Registration is not open for this event.";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        var alreadyRegistered = await _context.EventRegistrations
            .AnyAsync(r => r.VolunteerEventId == eventId && r.UserId == userId);

        if (alreadyRegistered)
        {
        TempData["Message"] = "You are already registered for this event.";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        if (volunteerEvent.Registrations.Count >= volunteerEvent.Capacity)
        {
        TempData["Message"] = "This event has reached its volunteer capacity.";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        _context.EventRegistrations.Add(new EventRegistration
        {
            VolunteerEventId = eventId,
            UserId = userId,
            RegistrationDate = DateTime.Now
        });

        var profile = await GetPrimaryProfileForUserAsync(userId);
        if (profile != null)
        {
            _context.Notifications.Add(new Notification
            {
                UserProfileId = profile.Id,
                Message = $"You registered for the event: {volunteerEvent.Title}."
            });
        }
        await _context.SaveChangesAsync();

        TempData["Message"] = "Successfully registered for the event!";
        return RedirectToAction(nameof(MyEvents));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unregister(int eventId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        var registration = await _context.EventRegistrations
            .FirstOrDefaultAsync(r => r.VolunteerEventId == eventId && r.UserId == userId);

        if (registration != null)
        {
            _context.EventRegistrations.Remove(registration);

            var profile = await GetPrimaryProfileForUserAsync(userId);
            var volunteerEvent = await _context.VolunteerEvents.FindAsync(eventId);
            if (profile != null && volunteerEvent != null)
            {
                _context.Notifications.Add(new Notification
                {
                    UserProfileId = profile.Id,
                    Message = $"You cancelled your registration for: {volunteerEvent.Title}."
                });
            }

            await _context.SaveChangesAsync();
        TempData["Message"] = "Successfully unregistered from the event.";
        }

        return RedirectToAction(nameof(MyEvents));
    }

    [AllowAnonymous]
    // Details page: loads one selected record with related data and checks permissions for the current user.
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        await AutoCloseExpiredEventsAsync();

        var volunteerEvent = await _context.VolunteerEvents
            .Include(e => e.OrganizerProfile)
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (volunteerEvent == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        ViewBag.IsRegistered = userId != null && await _context.EventRegistrations
            .AnyAsync(r => r.VolunteerEventId == id && r.UserId == userId);
        ViewBag.RegisteredCount = volunteerEvent.Registrations.Count;
        ViewBag.RemainingSlots = Math.Max(0, volunteerEvent.Capacity - volunteerEvent.Registrations.Count);

        var profile = await GetCurrentProfileAsync();
        var isAdmin = User.IsInRole("Admin");
        ViewBag.CurrentProfileId = profile?.Id;
        ViewBag.IsAdmin = isAdmin;
        ViewBag.CanManageEvent = CanManageEvent(volunteerEvent, profile, isAdmin);
        ViewBag.PublicStatus = volunteerEvent.PublicStatus;
        var organizerUserId = volunteerEvent.OrganizerProfile?.UserId;
        ViewBag.CanRate = userId != null && (ViewBag.IsRegistered == true) && !string.IsNullOrWhiteSpace(organizerUserId);
        ViewBag.MyRating = userId == null || string.IsNullOrWhiteSpace(organizerUserId) ? null : await _context.UserRatings
            .FirstOrDefaultAsync(r => r.EventId == volunteerEvent.Id && r.FromUserId == userId && r.ToUserId == organizerUserId);
        ViewBag.AverageRating = string.IsNullOrWhiteSpace(organizerUserId)
            ? 0
            : await _context.UserRatings
                .Where(r => r.EventId == volunteerEvent.Id && r.ToUserId == organizerUserId)
                .Select(r => (double?)r.Score)
                .AverageAsync() ?? 0;
        ViewBag.RatingCount = string.IsNullOrWhiteSpace(organizerUserId)
            ? 0
            : await _context.UserRatings.CountAsync(r => r.EventId == volunteerEvent.Id && r.ToUserId == organizerUserId);

        return View(volunteerEvent);
    }

    [Authorize(Roles = "Organizer,Admin")]
    // Create page/action: GET shows the form; POST validates input, saves new data, and redirects after success.
    public async Task<IActionResult> Create()
    {
        var profile = await GetCurrentProfileAsync();
        if (profile == null)
        {
        TempData["Message"] = "Please complete your profile before creating an event.";
            return RedirectToAction("Edit", "Profile");
        }

        if (User.IsInRole("Organizer") && string.IsNullOrWhiteSpace(profile.OrganizationName))
        {
        TempData["Message"] = "Please add your organization name to your profile before creating events.";
            return RedirectToAction("Edit", "Profile");
        }

        return View(new VolunteerEvent
        {
            EventDate = DateTime.Today.AddDays(7),
            EventTime = new TimeSpan(9, 0, 0),
            RegistrationOpenDate = DateTime.Today,
            RegistrationClosingDate = DateTime.Today.AddDays(6),
            Status = "Upcoming",
            Capacity = 10
        });
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    // Create page/action: GET shows the form; POST validates input, saves new data, and redirects after success.
    public async Task<IActionResult> Create(VolunteerEvent volunteerEvent, IFormFile? eventImage)
    {
        var userProfile = await GetCurrentProfileAsync();

        if (userProfile == null)
        {
        TempData["Message"] = "Please complete your user profile before creating an event.";
            return RedirectToAction("Edit", "Profile");
        }

        if (User.IsInRole("Organizer") && string.IsNullOrWhiteSpace(userProfile.OrganizationName))
        {
        TempData["Message"] = "Please add your organization name to your profile before creating events.";
            return RedirectToAction("Edit", "Profile");
        }

        volunteerEvent.OrganizerProfileId = userProfile.Id;
        volunteerEvent.Status = string.IsNullOrWhiteSpace(volunteerEvent.Status) ? "Upcoming" : volunteerEvent.Status;
        if (!volunteerEvent.RegistrationClosingDate.HasValue)
        {
            volunteerEvent.RegistrationClosingDate = volunteerEvent.EventDate;
        }

        ModelState.Remove(nameof(VolunteerEvent.OrganizerProfileId));
        ModelState.Remove(nameof(VolunteerEvent.OrganizerProfile));
        ModelState.Remove(nameof(VolunteerEvent.Registrations));
        ModelState.Remove(nameof(VolunteerEvent.Status));
        ModelState.Remove(nameof(VolunteerEvent.ImageUrl));
        ModelState.Remove("eventImage");

        if (volunteerEvent.RegistrationClosingDate.HasValue && volunteerEvent.RegistrationClosingDate.Value.Date < volunteerEvent.RegistrationOpenDate.Date)
        {
            ModelState.AddModelError(nameof(VolunteerEvent.RegistrationClosingDate), "Closing date cannot be before the opening date.");
        }

        if (!ModelState.IsValid)
        {
            return View(volunteerEvent);
        }

        var imageUrl = await SaveEventImageAsync(eventImage, null);
        if (imageUrl != null)
        {
            volunteerEvent.ImageUrl = imageUrl;
        }

        _context.VolunteerEvents.Add(volunteerEvent);

        var notifyProfiles = await _context.UserProfiles
            .Where(p => p.RoleName == "Volunteer")
            .ToListAsync();
        foreach (var profile in notifyProfiles)
        {
            _context.Notifications.Add(new Notification
            {
                UserProfileId = profile.Id,
                Message = $"New volunteer event available: {volunteerEvent.Title}."
            });
        }

        await _context.SaveChangesAsync();
        TempData["Message"] = "Event created successfully!";
        return RedirectToAction(nameof(MyCreatedEvents));
    }

    [Authorize(Roles = "Organizer,Admin")]
    // Edit page/action: GET loads existing data; POST validates ownership/role, updates fields, and saves changes.
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var volunteerEvent = await _context.VolunteerEvents
            .Include(e => e.OrganizerProfile)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (volunteerEvent == null) return NotFound();

        if (!await CanManageEventAsync(volunteerEvent))
        {
            return Forbid();
        }

        return View(volunteerEvent);
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    // Edit page/action: GET loads existing data; POST validates ownership/role, updates fields, and saves changes.
    public async Task<IActionResult> Edit(int id, VolunteerEvent volunteerEvent, IFormFile? eventImage)
    {
        if (id != volunteerEvent.Id) return NotFound();

        var existingEvent = await _context.VolunteerEvents
            .Include(e => e.OrganizerProfile)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (existingEvent == null) return NotFound();

        if (!await CanManageEventAsync(existingEvent))
        {
            return Forbid();
        }

        ModelState.Remove(nameof(VolunteerEvent.OrganizerProfile));
        ModelState.Remove(nameof(VolunteerEvent.Registrations));
        ModelState.Remove(nameof(VolunteerEvent.OrganizerProfileId));
        ModelState.Remove(nameof(VolunteerEvent.ImageUrl));
        ModelState.Remove("eventImage");

        if (volunteerEvent.RegistrationClosingDate.HasValue && volunteerEvent.RegistrationClosingDate.Value.Date < volunteerEvent.RegistrationOpenDate.Date)
        {
            ModelState.AddModelError(nameof(VolunteerEvent.RegistrationClosingDate), "Closing date cannot be before the opening date.");
        }

        if (!ModelState.IsValid)
        {
            volunteerEvent.OrganizerProfileId = existingEvent.OrganizerProfileId;
            volunteerEvent.ImageUrl = existingEvent.ImageUrl;
            return View(volunteerEvent);
        }

        existingEvent.Title = volunteerEvent.Title;
        existingEvent.Description = volunteerEvent.Description;
        existingEvent.Location = volunteerEvent.Location;
        existingEvent.EventDate = volunteerEvent.EventDate;
        existingEvent.EventTime = volunteerEvent.EventTime;
        existingEvent.RegistrationOpenDate = volunteerEvent.RegistrationOpenDate;
        existingEvent.RegistrationClosingDate = volunteerEvent.RegistrationClosingDate;
        existingEvent.Capacity = volunteerEvent.Capacity;
        existingEvent.Status = volunteerEvent.Status;

        var imageUrl = await SaveEventImageAsync(eventImage, existingEvent.ImageUrl);
        if (imageUrl != null)
        {
            existingEvent.ImageUrl = imageUrl;
        }

        await _context.SaveChangesAsync();
        TempData["Message"] = "Event updated successfully!";
        return RedirectToAction(nameof(Details), new { id = existingEvent.Id });
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    // Delete action: confirms permission, removes the selected record, and saves the database change.
    public async Task<IActionResult> Delete(int id)
    {
        var volunteerEvent = await _context.VolunteerEvents
            .Include(e => e.OrganizerProfile)
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (volunteerEvent == null) return NotFound();

        if (!await CanManageEventAsync(volunteerEvent))
        {
            return Forbid();
        }

        if (volunteerEvent.Registrations.Any())
        {
            _context.EventRegistrations.RemoveRange(volunteerEvent.Registrations);
        }

        DeleteLocalFile(volunteerEvent.ImageUrl, "events");
        _context.VolunteerEvents.Remove(volunteerEvent);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Event deleted successfully.";
        return RedirectToAction(nameof(MyCreatedEvents));
    }

    [Authorize(Roles = "Organizer,Admin")]
    // Shows the volunteer list registered for a selected event.
    public async Task<IActionResult> JoinedVolunteers(int id)
    {
        var volunteerEvent = await _context.VolunteerEvents
            .Include(e => e.OrganizerProfile)
            .Include(e => e.Registrations)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (volunteerEvent == null) return NotFound();
        if (!await CanManageEventAsync(volunteerEvent)) return Forbid();

        var registeredUserIds = volunteerEvent.Registrations.Select(r => r.UserId).ToList();
        var profiles = await _context.UserProfiles
            .Where(p => registeredUserIds.Contains(p.UserId))
            .OrderBy(p => p.FullName)
            .ToListAsync();

        var organizerUserId = _userManager.GetUserId(User) ?? string.Empty;
        var volunteerUserIds = profiles.Select(p => p.UserId).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();

        var existingRatings = await _context.UserRatings
            .Where(r => r.EventId == id && r.FromUserId == organizerUserId && volunteerUserIds.Contains(r.ToUserId))
            .ToListAsync();

        var ratingLookup = existingRatings.ToDictionary(r => r.ToUserId, r => r);
        var averageLookup = await _context.UserRatings
            .Where(r => volunteerUserIds.Contains(r.ToUserId))
            .GroupBy(r => r.ToUserId)
            .Select(g => new { UserId = g.Key, Average = g.Average(x => x.Score), Count = g.Count() })
            .ToListAsync();

        ViewBag.VolunteerProfiles = profiles;
        ViewBag.OrganizerRatings = ratingLookup;
        ViewBag.VolunteerAverageRatings = averageLookup.ToDictionary(x => x.UserId, x => x.Average);
        ViewBag.VolunteerRatingCounts = averageLookup.ToDictionary(x => x.UserId, x => x.Count);
        return View(volunteerEvent);
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendVolunteerNotification(int id, string message)
    {
        var volunteerEvent = await _context.VolunteerEvents
            .Include(e => e.OrganizerProfile)
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (volunteerEvent == null) return NotFound();
        if (!await CanManageEventAsync(volunteerEvent)) return Forbid();

        if (string.IsNullOrWhiteSpace(message))
        {
        TempData["Message"] = "Please enter a message before sending.";
            return RedirectToAction(nameof(JoinedVolunteers), new { id });
        }

        var registeredUserIds = volunteerEvent.Registrations.Select(r => r.UserId).ToList();
        var volunteerProfiles = await _context.UserProfiles
            .Where(p => registeredUserIds.Contains(p.UserId))
            .ToListAsync();

        foreach (var profile in volunteerProfiles)
        {
            _context.Notifications.Add(new Notification
            {
                UserProfileId = profile.Id,
                Message = $"Event notice for '{volunteerEvent.Title}': {message.Trim()}"
            });
        }

        await _context.SaveChangesAsync();
        TempData["Message"] = $"Notification sent to {volunteerProfiles.Count} registered volunteer(s).";
        return RedirectToAction(nameof(JoinedVolunteers), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RateEvent(int id, int score, string? reviewText)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        if (score < 1 || score > 5)
        {
        TempData["Message"] = "Please select a rating between 1 and 5.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var volunteerEvent = await _context.VolunteerEvents
            .Include(e => e.OrganizerProfile)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (volunteerEvent == null) return NotFound();

        var isRegistered = await _context.EventRegistrations.AnyAsync(r => r.VolunteerEventId == id && r.UserId == userId);
        if (!isRegistered && !User.IsInRole("Admin")) return Forbid();

        var toUserId = volunteerEvent.OrganizerProfile?.UserId ?? userId;
        var rating = await _context.UserRatings.FirstOrDefaultAsync(r => r.EventId == id && r.FromUserId == userId && r.ToUserId == toUserId);
        if (rating == null)
        {
            rating = new UserRating { EventId = id, FromUserId = userId, ToUserId = toUserId };
            _context.UserRatings.Add(rating);
        }

        rating.Score = score;
        rating.ReviewText = reviewText;
        await _context.SaveChangesAsync();

        TempData["Message"] = "Thank you. Your rating was saved.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RateVolunteer(int id, string volunteerUserId, int score, string? reviewText)
    {
        var volunteerEvent = await _context.VolunteerEvents
            .Include(e => e.OrganizerProfile)
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (volunteerEvent == null) return NotFound();
        if (!await CanManageEventAsync(volunteerEvent)) return Forbid();

        if (string.IsNullOrWhiteSpace(volunteerUserId))
        {
        TempData["Message"] = "Volunteer account was not found.";
            return RedirectToAction(nameof(JoinedVolunteers), new { id });
        }

        if (score < 1 || score > 5)
        {
        TempData["Message"] = "Please select a rating between 1 and 5.";
            return RedirectToAction(nameof(JoinedVolunteers), new { id });
        }

        var isRegisteredVolunteer = volunteerEvent.Registrations.Any(r => r.UserId == volunteerUserId);
        if (!isRegisteredVolunteer)
        {
        TempData["Message"] = "Only registered volunteers can be rated for this event.";
            return RedirectToAction(nameof(JoinedVolunteers), new { id });
        }

        var fromUserId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(fromUserId)) return RedirectToAction("Login", "Account");

        var rating = await _context.UserRatings.FirstOrDefaultAsync(r =>
            r.EventId == id && r.FromUserId == fromUserId && r.ToUserId == volunteerUserId);

        if (rating == null)
        {
            rating = new UserRating
            {
                EventId = id,
                FromUserId = fromUserId,
                ToUserId = volunteerUserId
            };
            _context.UserRatings.Add(rating);
        }

        rating.Score = score;
        rating.ReviewText = reviewText;

        await _context.SaveChangesAsync();
        TempData["Message"] = "Volunteer rating saved successfully.";
        return RedirectToAction(nameof(JoinedVolunteers), new { id });
    }

    private async Task<UserProfile?> GetCurrentProfileAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return null;
        return await GetPrimaryProfileForUserAsync(userId);
    }

    private async Task<UserProfile?> GetPrimaryProfileForUserAsync(string userId)
    {
        return await _context.UserProfiles
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsVerified)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    private async Task<bool> CanManageEventAsync(VolunteerEvent volunteerEvent)
    {
        if (User.IsInRole("Admin")) return true;

        var profile = await GetCurrentProfileAsync();
        if (profile == null) return false;

        if (volunteerEvent.OrganizerProfile == null)
        {
            volunteerEvent.OrganizerProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.Id == volunteerEvent.OrganizerProfileId);
        }

        return CanManageEvent(volunteerEvent, profile, false);
    }

    private static bool CanManageEvent(VolunteerEvent volunteerEvent, UserProfile? currentProfile, bool isAdmin)
    {
        if (isAdmin) return true;
        if (currentProfile == null) return false;

        // Final ownership rule: an Organizer can edit/delete only the event created by their own profile.
        // Organization-name matching is not used for authorization, because another user could type the same organization name.
        return volunteerEvent.OrganizerProfileId == currentProfile.Id;
    }

    private async Task<string?> SaveEventImageAsync(IFormFile? file, string? oldImageUrl)
    {
        if (file == null || file.Length == 0) return null;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError("", "Only JPG, JPEG, PNG, and WEBP event images are allowed.");
            return null;
        }

        if (file.Length > 4 * 1024 * 1024)
        {
            ModelState.AddModelError("", "Event image size must be less than 4MB.");
            return null;
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "events");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString() + extension;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        DeleteLocalFile(oldImageUrl, "events");
        return "/uploads/events/" + uniqueFileName;
    }

    private void DeleteLocalFile(string? relativeUrl, string folderName)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl)) return;

        var relativePath = relativeUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
        var fullPath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, relativePath));
        var uploadRoot = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "uploads", folderName));

        if (fullPath.StartsWith(uploadRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }

    private async Task AutoCloseExpiredEventsAsync()
    {
        var today = DateTime.Today;
        var expiredEvents = await _context.VolunteerEvents
            .Where(e => e.RegistrationClosingDate.HasValue
                        && e.RegistrationClosingDate.Value.Date < today
                        && e.Status != "Closed"
                        && e.Status != "Completed"
                        && e.Status != "Cancelled")
            .ToListAsync();

        if (!expiredEvents.Any()) return;

        foreach (var item in expiredEvents)
        {
            item.Status = "Closed";
        }

        await _context.SaveChangesAsync();
    }
}
