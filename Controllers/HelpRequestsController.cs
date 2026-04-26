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
        var isAdmin = User.IsInRole("Admin");

        var query = _context.HelpRequests
            .Include(h => h.SubmittedBy)
            .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(h => profile != null && h.UserProfileId == profile.Id);
        }

        var requests = await query
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

        ViewBag.CurrentProfileId = profile?.Id;
        ViewBag.IsAdmin = isAdmin;

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

        if (!await CanViewHelpRequestAsync(helpRequest)) return Forbid();

        ViewBag.CanManageHelpRequest = await CanManageHelpRequestAsync(helpRequest);
        ViewBag.IsAdmin = User.IsInRole("Admin");

        return View(helpRequest);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var helpRequest = await _context.HelpRequests.FirstOrDefaultAsync(h => h.Id == id);
        if (helpRequest == null) return NotFound();

        if (!await CanManageHelpRequestAsync(helpRequest))
        {
            return Forbid();
        }

        return View(helpRequest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, HelpRequest helpRequest)
    {
        if (id != helpRequest.Id) return NotFound();

        var existingRequest = await _context.HelpRequests.FirstOrDefaultAsync(h => h.Id == id);
        if (existingRequest == null) return NotFound();

        if (!await CanManageHelpRequestAsync(existingRequest))
        {
            return Forbid();
        }

        ModelState.Remove(nameof(HelpRequest.SubmittedBy));
        ModelState.Remove(nameof(HelpRequest.UserProfileId));
        ModelState.Remove(nameof(HelpRequest.Status));

        if (!ModelState.IsValid)
        {
            helpRequest.UserProfileId = existingRequest.UserProfileId;
            helpRequest.Status = existingRequest.Status;
            return View(helpRequest);
        }

        existingRequest.Title = helpRequest.Title;
        existingRequest.Description = helpRequest.Description;

        await _context.SaveChangesAsync();

        TempData["Message"] = "Help request updated successfully.";
        return RedirectToAction(nameof(Details), new { id = existingRequest.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var helpRequest = await _context.HelpRequests.FirstOrDefaultAsync(h => h.Id == id);
        if (helpRequest == null) return NotFound();

        if (!await CanManageHelpRequestAsync(helpRequest))
        {
            return Forbid();
        }

        _context.HelpRequests.Remove(helpRequest);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Help request deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
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

        if (!await CanManageHelpRequestAsync(helpRequest))
        {
            return Forbid();
        }

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

    private async Task<bool> CanViewHelpRequestAsync(HelpRequest helpRequest)
    {
        if (User.IsInRole("Admin")) return true;

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        return profile != null && helpRequest.UserProfileId == profile.Id;
    }

    private async Task<bool> CanManageHelpRequestAsync(HelpRequest helpRequest)
    {
        if (User.IsInRole("Admin")) return true;

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        return profile != null && helpRequest.UserProfileId == profile.Id;
    }
}
