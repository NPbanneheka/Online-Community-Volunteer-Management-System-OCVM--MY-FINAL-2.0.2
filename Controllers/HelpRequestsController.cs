using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;

namespace OCVMS.Controllers;

[Authorize]
public class HelpRequestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public HelpRequestsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);

        var query = _context.HelpRequests
            .Include(h => h.SubmittedBy)
            .AsQueryable();

        if (!User.IsInRole("Admin") && !User.IsInRole("Organizer"))
        {
            query = query.Where(h => profile != null && h.UserProfileId == profile.Id);
        }

        var requests = await query
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

        return View(requests);
    }

    public IActionResult Create()
    {
        return View(new HelpRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HelpRequest helpRequest)
    {
        var userId = _userManager.GetUserId(User);
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null)
        {
            TempData["Message"] = "Please complete your profile before submitting a help request.";
            return RedirectToAction("Edit", "Profile");
        }

        helpRequest.UserProfileId = profile.Id;
        helpRequest.Status = "Pending";

        ModelState.Remove(nameof(HelpRequest.SubmittedBy));
        ModelState.Remove(nameof(HelpRequest.UserProfileId));
        ModelState.Remove(nameof(HelpRequest.Status));

        if (!ModelState.IsValid)
        {
            return View(helpRequest);
        }

        _context.HelpRequests.Add(helpRequest);

        var notifyProfiles = await _context.UserProfiles
            .Where(p => p.RoleName == "Admin" || p.RoleName == "Organizer")
            .ToListAsync();

        foreach (var notifyProfile in notifyProfiles)
        {
            _context.Notifications.Add(new Notification
            {
                UserProfileId = notifyProfile.Id,
                Message = $"New help request submitted: {helpRequest.Title}."
            });
        }

        await _context.SaveChangesAsync();

        TempData["Message"] = "Help request submitted successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var helpRequest = await _context.HelpRequests
            .Include(h => h.SubmittedBy)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (helpRequest == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);

        var canView = User.IsInRole("Admin")
            || User.IsInRole("Organizer")
            || (profile != null && profile.Id == helpRequest.UserProfileId);

        if (!canView) return Forbid();

        return View(helpRequest);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organizer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var allowedStatuses = new[] { "Pending", "In Progress", "Resolved", "Closed" };
        if (!allowedStatuses.Contains(status))
        {
            TempData["Message"] = "Invalid status selected.";
            return RedirectToAction(nameof(Index));
        }

        var helpRequest = await _context.HelpRequests.FindAsync(id);
        if (helpRequest == null) return NotFound();

        helpRequest.Status = status;

        _context.Notifications.Add(new Notification
        {
            UserProfileId = helpRequest.UserProfileId,
            Message = $"Your help request '{helpRequest.Title}' status changed to {status}."
        });

        await _context.SaveChangesAsync();

        TempData["Message"] = "Help request status updated.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
