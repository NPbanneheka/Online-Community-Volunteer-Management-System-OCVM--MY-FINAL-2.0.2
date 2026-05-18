// Controllers/EventsController.cs
// This MVC controller file that receives browser requests and coordinates models, services, and views.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// ASP.NET Core authorization attributes such as [Authorize] and [AllowAnonymous].
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
// ASP.NET Core Identity services for users, roles, passwords, sign-in sessions, and lockout/ban behavior.
using Microsoft.AspNetCore.Identity;
// ASP.NET Core MVC base classes and action results used by controllers.
using Microsoft.AspNetCore.Mvc;
// Entity Framework Core features such as Include(), Where(), ToListAsync(), and database queries.
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Controllers;

[Authorize]
// Controller class: methods inside this class respond to user actions from the browser.
public class EventsController : Controller
{
    private const string ApplicationRatingTargetId = "__APPLICATION__";
    private const int ApplicationRatingEventId = 0;

    // _context connects this class to SQL Server through Entity Framework Core and ApplicationDbContext.
    private readonly ApplicationDbContext _context;
    // _userManager works with ASP.NET Identity users, including lookup, creation, roles, and passwords.
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public EventsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
    }

    [AllowAnonymous]
    // Loads the default page or list view for this controller.
    public async Task<IActionResult> Index(string? searchTerm, string? statusFilter, DateTime? eventDate)
    {
        await AutoCloseExpiredEventsAsync();

        var eventsQuery = _context.VolunteerEvents
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.OrganizerProfile)
            // Include loads related table data so the view can access connected records without extra queries.
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
        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
        var isAdmin = User.IsInRole("Admin");

        ViewBag.CurrentProfileId = currentProfile?.Id;
        ViewBag.CurrentOrganizationName = currentProfile?.OrganizationName;
        ViewBag.IsAdmin = isAdmin;
        ViewBag.ManageableEventIds = eventsList
            .Where(e => CanManageEvent(e, currentProfile, isAdmin))
            .Select(e => e.Id)
            .ToList();

        // Reads the currently logged-in Identity user ID from the authentication cookie.
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
        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(eventsList);
    }

    // Loads events joined by the currently logged-in volunteer.

    public async Task<IActionResult> MyEvents()
    {
        await AutoCloseExpiredEventsAsync();

        // Reads the currently logged-in Identity user ID from the authentication cookie.
        var userId = _userManager.GetUserId(User);
        // Handles missing data safely before continuing with the requested operation.
        // Redirects the browser to another MVC action after the current operation is complete.
        if (userId == null) return RedirectToAction("Login", "Account");

        var myRegisteredEvents = await _context.EventRegistrations
            .Where(r => r.UserId == userId)
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(r => r.VolunteerEvent)
                // ThenInclude loads deeper related data from a previously included navigation property.
                .ThenInclude(e => e!.OrganizerProfile)
            .Select(r => r.VolunteerEvent!)
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.EventTime)
            .ToListAsync();

        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(myRegisteredEvents);
    }

    [Authorize(Roles = "Organizer,Admin")]
    // Loads events created by the current organizer profile.
    public async Task<IActionResult> MyCreatedEvents()
    {
        await AutoCloseExpiredEventsAsync();

        var profile = await GetCurrentProfileAsync();
        // Handles missing data safely before continuing with the requested operation.
        if (profile == null)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Please complete your profile before managing events.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction("Edit", "Profile");
        }

        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
        var isAdmin = User.IsInRole("Admin");
        var events = await _context.VolunteerEvents
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.OrganizerProfile)
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.Registrations)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        events = events
            .Where(e => CanManageEvent(e, profile, isAdmin))
            .ToList();

        ViewBag.CurrentProfileId = profile.Id;
        ViewBag.CurrentOrganizationName = profile.OrganizationName;
        ViewBag.IsAdmin = isAdmin;

        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(events);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Handles user registration and stores both Identity login data and OCVMS profile data.
    public async Task<IActionResult> Register(int eventId)
    {
        await AutoCloseExpiredEventsAsync();

        // Reads the currently logged-in Identity user ID from the authentication cookie.
        var userId = _userManager.GetUserId(User);
        // Handles missing data safely before continuing with the requested operation.
        // Redirects the browser to another MVC action after the current operation is complete.
        if (userId == null) return RedirectToAction("Login", "Account");

        var volunteerEvent = await _context.VolunteerEvents
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (volunteerEvent == null) return NotFound();

        if (!volunteerEvent.IsRegistrationOpen)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Registration is not open for this event.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        var alreadyRegistered = await _context.EventRegistrations
            .AnyAsync(r => r.VolunteerEventId == eventId && r.UserId == userId);

        if (alreadyRegistered)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "You are already registered for this event.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        if (volunteerEvent.Registrations.Count >= volunteerEvent.Capacity)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "This event has reached its volunteer capacity.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        // Adds a new entity to EF Core change tracking so it can be inserted into the database.
        // Creates a registration entity connecting a volunteer profile to an event.
        _context.EventRegistrations.Add(new EventRegistration
        {
            VolunteerEventId = eventId,
            UserId = userId,
            RegistrationDate = DateTime.Now
        });

        var profile = await GetPrimaryProfileForUserAsync(userId);
        if (profile != null)
        {
            // Adds a new entity to EF Core change tracking so it can be inserted into the database.
            // Creates a notification record connected to a user profile.
            _context.Notifications.Add(new Notification
            {
                UserProfileId = profile.Id,
                Message = $"You registered for the event: {volunteerEvent.Title}."
            });
        }

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Successfully registered for the event!";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(MyEvents));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Handles the Unregister request using MVC action logic and returns the appropriate response.
    public async Task<IActionResult> Unregister(int eventId)
    {
        // Reads the currently logged-in Identity user ID from the authentication cookie.
        var userId = _userManager.GetUserId(User);
        // Handles missing data safely before continuing with the requested operation.
        // Redirects the browser to another MVC action after the current operation is complete.
        if (userId == null) return RedirectToAction("Login", "Account");

        var registration = await _context.EventRegistrations
            .FirstOrDefaultAsync(r => r.VolunteerEventId == eventId && r.UserId == userId);

        if (registration != null)
        {
            // Marks the selected entity for deletion from the database.
            _context.EventRegistrations.Remove(registration);

            var profile = await GetPrimaryProfileForUserAsync(userId);
            var volunteerEvent = await _context.VolunteerEvents.FindAsync(eventId);
            if (profile != null && volunteerEvent != null)
            {
                // Adds a new entity to EF Core change tracking so it can be inserted into the database.
                // Creates a notification record connected to a user profile.
                _context.Notifications.Add(new Notification
                {
                    UserProfileId = profile.Id,
                    Message = $"You cancelled your registration for: {volunteerEvent.Title}."
                });
            }

            // Commits all pending EF Core changes to the SQL Server database.
            await _context.SaveChangesAsync();
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Successfully unregistered from the event.";
        }

        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(MyEvents));
    }

    [AllowAnonymous]
    // Loads a single record with related data for a detail page.
    public async Task<IActionResult> Details(int? id)
    {
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (id == null) return NotFound();
        await AutoCloseExpiredEventsAsync();

        var volunteerEvent = await _context.VolunteerEvents
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.OrganizerProfile)
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(m => m.Id == id);

        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (volunteerEvent == null) return NotFound();

        // Reads the currently logged-in Identity user ID from the authentication cookie.
        var userId = _userManager.GetUserId(User);
        ViewBag.IsRegistered = userId != null && await _context.EventRegistrations
            .AnyAsync(r => r.VolunteerEventId == id && r.UserId == userId);
        ViewBag.RegisteredCount = volunteerEvent.Registrations.Count;
        ViewBag.RemainingSlots = Math.Max(0, volunteerEvent.Capacity - volunteerEvent.Registrations.Count);

        var profile = await GetCurrentProfileAsync();
        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
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

        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(volunteerEvent);
    }

    [Authorize(Roles = "Organizer,Admin")]
    // Displays or processes the form used to create a new record.
    public async Task<IActionResult> Create()
    {
        var profile = await GetCurrentProfileAsync();
        // Handles missing data safely before continuing with the requested operation.
        if (profile == null)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Please complete your profile before creating an event.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction("Edit", "Profile");
        }

        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
        if (User.IsInRole("Organizer") && string.IsNullOrWhiteSpace(profile.OrganizationName))
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Please add your organization name to your profile before creating events.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction("Edit", "Profile");
        }

        // Sends data to a Razor view so the page can be rendered in the browser.
        // Creates an event entity that will be stored in the VolunteerEvents table.
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
    // Displays or processes the form used to create a new record.
    public async Task<IActionResult> Create(VolunteerEvent volunteerEvent, IFormFile? eventImage)
    {
        var userProfile = await GetCurrentProfileAsync();

        // Handles missing data safely before continuing with the requested operation.
        if (userProfile == null)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Please complete your user profile before creating an event.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction("Edit", "Profile");
        }

        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
        if (User.IsInRole("Organizer") && string.IsNullOrWhiteSpace(userProfile.OrganizationName))
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Please add your organization name to your profile before creating events.";
            // Redirects the browser to another MVC action after the current operation is complete.
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

        // Stops processing when form validation fails and returns the same view with validation messages.
        if (!ModelState.IsValid)
        {
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(volunteerEvent);
        }

        var imageUrl = await SaveEventImageAsync(eventImage, null);
        if (imageUrl != null)
        {
            volunteerEvent.ImageUrl = imageUrl;
        }

        // Adds a new entity to EF Core change tracking so it can be inserted into the database.
        _context.VolunteerEvents.Add(volunteerEvent);

        var notifyProfiles = await _context.UserProfiles
            .Where(p => p.RoleName == "Volunteer")
            .ToListAsync();
        foreach (var profile in notifyProfiles)
        {
            // Adds a new entity to EF Core change tracking so it can be inserted into the database.
            // Creates a notification record connected to a user profile.
            _context.Notifications.Add(new Notification
            {
                UserProfileId = profile.Id,
                Message = $"New volunteer event available: {volunteerEvent.Title}."
            });
        }

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();
        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Event created successfully!";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(MyCreatedEvents));
    }

    [Authorize(Roles = "Organizer,Admin")]
    // Displays or processes the form used to update an existing record.
    public async Task<IActionResult> Edit(int? id)
    {
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (id == null) return NotFound();

        var volunteerEvent = await _context.VolunteerEvents
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.OrganizerProfile)
            .FirstOrDefaultAsync(e => e.Id == id);
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (volunteerEvent == null) return NotFound();

        // Enforces ownership or role-based permission before allowing data changes.
        if (!await CanManageEventAsync(volunteerEvent))
        {
            // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
            return Forbid();
        }

        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(volunteerEvent);
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    // Displays or processes the form used to update an existing record.
    public async Task<IActionResult> Edit(int id, VolunteerEvent volunteerEvent, IFormFile? eventImage)
    {
        // Returns HTTP 404 when the requested record does not exist.
        if (id != volunteerEvent.Id) return NotFound();

        var existingEvent = await _context.VolunteerEvents
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.OrganizerProfile)
            .FirstOrDefaultAsync(e => e.Id == id);
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (existingEvent == null) return NotFound();

        // Enforces ownership or role-based permission before allowing data changes.
        if (!await CanManageEventAsync(existingEvent))
        {
            // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
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

        // Stops processing when form validation fails and returns the same view with validation messages.
        if (!ModelState.IsValid)
        {
            volunteerEvent.OrganizerProfileId = existingEvent.OrganizerProfileId;
            volunteerEvent.ImageUrl = existingEvent.ImageUrl;
            // Sends data to a Razor view so the page can be rendered in the browser.
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

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();
        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Event updated successfully!";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(Details), new { id = existingEvent.Id });
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    // Deletes an existing record after validation and authorization checks.
    public async Task<IActionResult> Delete(int id)
    {
        var volunteerEvent = await _context.VolunteerEvents
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.OrganizerProfile)
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id);

        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (volunteerEvent == null) return NotFound();

        // Enforces ownership or role-based permission before allowing data changes.
        if (!await CanManageEventAsync(volunteerEvent))
        {
            // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
            return Forbid();
        }

        if (volunteerEvent.Registrations.Any())
        {
            // Marks multiple related entities for deletion in one database save operation.
            _context.EventRegistrations.RemoveRange(volunteerEvent.Registrations);
        }

        DeleteLocalFile(volunteerEvent.ImageUrl, "events");
        // Marks the selected entity for deletion from the database.
        _context.VolunteerEvents.Remove(volunteerEvent);
        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Event deleted successfully.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(MyCreatedEvents));
    }

    [Authorize(Roles = "Organizer,Admin")]
    // Shows volunteers who have registered for a selected event.
    public async Task<IActionResult> JoinedVolunteers(int id)
    {
        var volunteerEvent = await _context.VolunteerEvents
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.OrganizerProfile)
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.Registrations)
                // ThenInclude loads deeper related data from a previously included navigation property.
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (volunteerEvent == null) return NotFound();
        // Enforces ownership or role-based permission before allowing data changes.
        // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
        if (!await CanManageEventAsync(volunteerEvent)) return Forbid();

        var registeredUserIds = volunteerEvent.Registrations.Select(r => r.UserId).ToList();
        var profiles = await _context.UserProfiles
            .Where(p => registeredUserIds.Contains(p.UserId))
            .OrderBy(p => p.FullName)
            .ToListAsync();

        // Reads the currently logged-in Identity user ID from the authentication cookie.
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
        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(volunteerEvent);
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    // Handles the SendVolunteerNotification request using MVC action logic and returns the appropriate response.
    public async Task<IActionResult> SendVolunteerNotification(int id, string message)
    {
        var volunteerEvent = await _context.VolunteerEvents
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.OrganizerProfile)
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id);

        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (volunteerEvent == null) return NotFound();
        // Enforces ownership or role-based permission before allowing data changes.
        // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
        if (!await CanManageEventAsync(volunteerEvent)) return Forbid();

        if (string.IsNullOrWhiteSpace(message))
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Please enter a message before sending.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(JoinedVolunteers), new { id });
        }

        var registeredUserIds = volunteerEvent.Registrations.Select(r => r.UserId).ToList();
        var volunteerProfiles = await _context.UserProfiles
            .Where(p => registeredUserIds.Contains(p.UserId))
            .ToListAsync();

        foreach (var profile in volunteerProfiles)
        {
            // Adds a new entity to EF Core change tracking so it can be inserted into the database.
            // Creates a notification record connected to a user profile.
            _context.Notifications.Add(new Notification
            {
                UserProfileId = profile.Id,
                Message = $"Event notice for '{volunteerEvent.Title}': {message.Trim()}"
            });
        }

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();
        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = $"Notification sent to {volunteerProfiles.Count} registered volunteer(s).";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(JoinedVolunteers), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Handles the RateEvent request using MVC action logic and returns the appropriate response.
    public async Task<IActionResult> RateEvent(int id, int score, string? reviewText)
    {
        // Reads the currently logged-in Identity user ID from the authentication cookie.
        var userId = _userManager.GetUserId(User);
        // Handles missing data safely before continuing with the requested operation.
        // Redirects the browser to another MVC action after the current operation is complete.
        if (userId == null) return RedirectToAction("Login", "Account");

        if (score < 1 || score > 5)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Please select a rating between 1 and 5.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(Details), new { id });
        }

        var volunteerEvent = await _context.VolunteerEvents
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.OrganizerProfile)
            .FirstOrDefaultAsync(e => e.Id == id);
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (volunteerEvent == null) return NotFound();

        var isRegistered = await _context.EventRegistrations.AnyAsync(r => r.VolunteerEventId == id && r.UserId == userId);
        // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
        if (!isRegistered && !User.IsInRole("Admin")) return Forbid();

        var toUserId = volunteerEvent.OrganizerProfile?.UserId ?? userId;
        // Retrieves a single matching database record asynchronously; returns null when not found.
        var rating = await _context.UserRatings.FirstOrDefaultAsync(r => r.EventId == id && r.FromUserId == userId && r.ToUserId == toUserId);
        // Handles missing data safely before continuing with the requested operation.
        if (rating == null)
        {
            rating = new UserRating { EventId = id, FromUserId = userId, ToUserId = toUserId };
            // Adds a new entity to EF Core change tracking so it can be inserted into the database.
            _context.UserRatings.Add(rating);
        }

        rating.Score = score;
        rating.ReviewText = reviewText;
        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Thank you. Your rating was saved.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    // Handles the RateVolunteer request using MVC action logic and returns the appropriate response.
    public async Task<IActionResult> RateVolunteer(int id, string volunteerUserId, int score, string? reviewText)
    {
        var volunteerEvent = await _context.VolunteerEvents
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.OrganizerProfile)
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id);

        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (volunteerEvent == null) return NotFound();
        // Enforces ownership or role-based permission before allowing data changes.
        // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
        if (!await CanManageEventAsync(volunteerEvent)) return Forbid();

        if (string.IsNullOrWhiteSpace(volunteerUserId))
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Volunteer account was not found.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(JoinedVolunteers), new { id });
        }

        if (score < 1 || score > 5)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Please select a rating between 1 and 5.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(JoinedVolunteers), new { id });
        }

        var isRegisteredVolunteer = volunteerEvent.Registrations.Any(r => r.UserId == volunteerUserId);
        if (!isRegisteredVolunteer)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Only registered volunteers can be rated for this event.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(JoinedVolunteers), new { id });
        }

        // Reads the currently logged-in Identity user ID from the authentication cookie.
        var fromUserId = _userManager.GetUserId(User);
        // Redirects the browser to another MVC action after the current operation is complete.
        if (string.IsNullOrWhiteSpace(fromUserId)) return RedirectToAction("Login", "Account");

        // Retrieves a single matching database record asynchronously; returns null when not found.
        var rating = await _context.UserRatings.FirstOrDefaultAsync(r =>
            r.EventId == id && r.FromUserId == fromUserId && r.ToUserId == volunteerUserId);

        // Handles missing data safely before continuing with the requested operation.
        if (rating == null)
        {
            rating = new UserRating
            {
                EventId = id,
                FromUserId = fromUserId,
                ToUserId = volunteerUserId
            };
            // Adds a new entity to EF Core change tracking so it can be inserted into the database.
            _context.UserRatings.Add(rating);
        }

        rating.Score = score;
        rating.ReviewText = reviewText;

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();
        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Volunteer rating saved successfully.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(JoinedVolunteers), new { id });
    }

    // Handles the GetCurrentProfileAsync request using MVC action logic and returns the appropriate response.

    private async Task<UserProfile?> GetCurrentProfileAsync()
    {
        // Reads the currently logged-in Identity user ID from the authentication cookie.
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return null;
        return await GetPrimaryProfileForUserAsync(userId);
    }

    // Handles the GetPrimaryProfileForUserAsync request using MVC action logic and returns the appropriate response.

    private async Task<UserProfile?> GetPrimaryProfileForUserAsync(string userId)
    {
        return await _context.UserProfiles
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsVerified)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    // Handles the CanManageEventAsync request using MVC action logic and returns the appropriate response.

    private async Task<bool> CanManageEventAsync(VolunteerEvent volunteerEvent)
    {
        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
        if (User.IsInRole("Admin")) return true;

        var profile = await GetCurrentProfileAsync();
        // Handles missing data safely before continuing with the requested operation.
        if (profile == null) return false;

        // Handles missing data safely before continuing with the requested operation.
        if (volunteerEvent.OrganizerProfile == null)
        {
            volunteerEvent.OrganizerProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.Id == volunteerEvent.OrganizerProfileId);
        }

        return CanManageEvent(volunteerEvent, profile, false);
    }

    // Handles the CanManageEvent request using MVC action logic and returns the appropriate response.

    private static bool CanManageEvent(VolunteerEvent volunteerEvent, UserProfile? currentProfile, bool isAdmin)
    {
        if (isAdmin) return true;
        // Handles missing data safely before continuing with the requested operation.
        if (currentProfile == null) return false;

        // Final ownership rule: an Organizer can edit/delete only the event created by their own profile.
        // Organization-name matching is not used for authorization, because another user could type the same organization name.
        return volunteerEvent.OrganizerProfileId == currentProfile.Id;
    }

    // Handles the SaveEventImageAsync request using MVC action logic and returns the appropriate response.

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

    // Handles the DeleteLocalFile request using MVC action logic and returns the appropriate response.

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

    // Handles the AutoCloseExpiredEventsAsync request using MVC action logic and returns the appropriate response.

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

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();
    }
}
