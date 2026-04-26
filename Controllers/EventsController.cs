using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;

namespace OCVMS.Controllers;

[Authorize]
public class EventsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public EventsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchTerm)
    {
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
                e.Description.Contains(term));
        }

        var eventsList = await eventsQuery
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.EventTime)
            .ToListAsync();

        var userId = _userManager.GetUserId(User);
        var currentProfile = userId == null
            ? null
            : await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);

        ViewBag.CurrentProfileId = currentProfile?.Id;
        ViewBag.IsAdmin = User.IsInRole("Admin");
        ViewBag.UserRegisteredEvents = userId == null
            ? new List<int>()
            : await _context.EventRegistrations
                .Where(r => r.UserId == userId)
                .Select(r => r.VolunteerEventId)
                .ToListAsync();

        ViewBag.SearchTerm = searchTerm;
        return View(eventsList);
    }

    public async Task<IActionResult> MyEvents()
    {
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
    public async Task<IActionResult> MyCreatedEvents()
    {
        var userId = _userManager.GetUserId(User);
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null)
        {
            TempData["Message"] = "Please complete your profile before managing events.";
            return RedirectToAction("Edit", "Profile");
        }

        var isAdmin = User.IsInRole("Admin");
        var events = await _context.VolunteerEvents
            .Include(e => e.Registrations)
            .Where(e => isAdmin || e.OrganizerProfileId == profile.Id)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        ViewBag.CurrentProfileId = profile.Id;
        ViewBag.IsAdmin = isAdmin;

        return View(events);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int eventId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        var volunteerEvent = await _context.VolunteerEvents
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (volunteerEvent == null)
        {
            return NotFound();
        }

        if (string.Equals(volunteerEvent.Status, "Closed", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Message"] = "This event is currently closed for registrations.";
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

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
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

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
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
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

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

        var profile = userId == null ? null : await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        ViewBag.CurrentProfileId = profile?.Id;
        ViewBag.IsAdmin = User.IsInRole("Admin");
        ViewBag.CanManageEvent = await CanManageEventAsync(volunteerEvent);

        return View(volunteerEvent);
    }

    [Authorize(Roles = "Organizer,Admin")]
    public IActionResult Create()
    {
        return View(new VolunteerEvent
        {
            EventDate = DateTime.Today.AddDays(7),
            EventTime = new TimeSpan(9, 0, 0),
            Status = "Upcoming",
            Capacity = 10
        });
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VolunteerEvent volunteerEvent)
    {
        var identityUserId = _userManager.GetUserId(User);
        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == identityUserId);

        if (userProfile == null)
        {
            TempData["Message"] = "Please complete your user profile before creating an event.";
            return RedirectToAction("Edit", "Profile");
        }

        volunteerEvent.OrganizerProfileId = userProfile.Id;
        volunteerEvent.Status = string.IsNullOrWhiteSpace(volunteerEvent.Status) ? "Upcoming" : volunteerEvent.Status;

        ModelState.Remove(nameof(VolunteerEvent.OrganizerProfileId));
        ModelState.Remove(nameof(VolunteerEvent.OrganizerProfile));
        ModelState.Remove(nameof(VolunteerEvent.Registrations));
        ModelState.Remove(nameof(VolunteerEvent.Status));

        if (!ModelState.IsValid)
        {
            return View(volunteerEvent);
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
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Organizer,Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var volunteerEvent = await _context.VolunteerEvents.FindAsync(id);
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
    public async Task<IActionResult> Edit(int id, VolunteerEvent volunteerEvent)
    {
        if (id != volunteerEvent.Id) return NotFound();

        var existingEvent = await _context.VolunteerEvents.FindAsync(id);
        if (existingEvent == null) return NotFound();

        if (!await CanManageEventAsync(existingEvent))
        {
            return Forbid();
        }

        ModelState.Remove(nameof(VolunteerEvent.OrganizerProfile));
        ModelState.Remove(nameof(VolunteerEvent.Registrations));
        ModelState.Remove(nameof(VolunteerEvent.OrganizerProfileId));

        if (!ModelState.IsValid)
        {
            volunteerEvent.OrganizerProfileId = existingEvent.OrganizerProfileId;
            return View(volunteerEvent);
        }

        existingEvent.Title = volunteerEvent.Title;
        existingEvent.Description = volunteerEvent.Description;
        existingEvent.Location = volunteerEvent.Location;
        existingEvent.EventDate = volunteerEvent.EventDate;
        existingEvent.EventTime = volunteerEvent.EventTime;
        existingEvent.Capacity = volunteerEvent.Capacity;
        existingEvent.Status = volunteerEvent.Status;
        existingEvent.ImageUrl = volunteerEvent.ImageUrl;

        await _context.SaveChangesAsync();
        TempData["Message"] = "Event updated successfully!";
        return RedirectToAction(nameof(Details), new { id = existingEvent.Id });
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var volunteerEvent = await _context.VolunteerEvents
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

        _context.VolunteerEvents.Remove(volunteerEvent);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Event deleted successfully.";
        return RedirectToAction(nameof(MyCreatedEvents));
    }

    private async Task<bool> CanManageEventAsync(VolunteerEvent volunteerEvent)
    {
        if (User.IsInRole("Admin")) return true;

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        return profile != null && volunteerEvent.OrganizerProfileId == profile.Id;
    }
}
