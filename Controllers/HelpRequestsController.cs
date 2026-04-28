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
        var profile = await GetCurrentProfileAsync();
        var isAdmin = User.IsInRole("Admin");

        var allRequests = await _context.HelpRequests
            .Include(h => h.SubmittedBy)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

        var requests = allRequests
            .Where(h => CanViewHelpRequest(h, profile, isAdmin))
            .ToList();

        ViewBag.CurrentProfileId = profile?.Id;
        ViewBag.CurrentOrganizationName = profile?.OrganizationName;
        ViewBag.IsAdmin = isAdmin;
        ViewBag.ManageableHelpRequestIds = requests
            .Where(h => CanManageHelpRequest(h, profile, isAdmin))
            .Select(h => h.Id)
            .ToList();
        ViewBag.StatusManageableHelpRequestIds = requests
            .Where(h => CanUpdateHelpRequestStatus(h, profile, isAdmin))
            .Select(h => h.Id)
            .ToList();

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
        var profile = await GetCurrentProfileAsync();

        if (profile == null)
        {
            TempData["Message"] = "Please complete your profile before submitting a support request.";
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
            .Where(p => p.RoleName == "Admin")
            .ToListAsync();

        foreach (var notifyProfile in notifyProfiles)
        {
            _context.Notifications.Add(new Notification
            {
                UserProfileId = notifyProfile.Id,
                Message = $"New support request submitted: {helpRequest.Title}."
            });
        }

        await _context.SaveChangesAsync();

        TempData["Message"] = "Support request submitted successfully.";
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
        ViewBag.CanUpdateHelpRequestStatus = await CanUpdateHelpRequestStatusAsync(helpRequest);
        ViewBag.IsAdmin = User.IsInRole("Admin");

        return View(helpRequest);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var helpRequest = await _context.HelpRequests
            .Include(h => h.SubmittedBy)
            .FirstOrDefaultAsync(h => h.Id == id);
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

        var existingRequest = await _context.HelpRequests
            .Include(h => h.SubmittedBy)
            .FirstOrDefaultAsync(h => h.Id == id);
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

        TempData["Message"] = "Support request updated successfully.";
        return RedirectToAction(nameof(Details), new { id = existingRequest.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var helpRequest = await _context.HelpRequests
            .Include(h => h.SubmittedBy)
            .FirstOrDefaultAsync(h => h.Id == id);
        if (helpRequest == null) return NotFound();

        if (!await CanManageHelpRequestAsync(helpRequest))
        {
            return Forbid();
        }

        _context.HelpRequests.Remove(helpRequest);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Support request deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var allowedStatuses = new[] { "Pending", "In Progress", "Resolved", "Closed" };
        if (!allowedStatuses.Contains(status))
        {
            TempData["Message"] = "Invalid status selected.";
            return RedirectToAction(nameof(Index));
        }

        var helpRequest = await _context.HelpRequests
            .Include(h => h.SubmittedBy)
            .FirstOrDefaultAsync(h => h.Id == id);
        if (helpRequest == null) return NotFound();

        if (!await CanUpdateHelpRequestStatusAsync(helpRequest))
        {
            return Forbid();
        }

        helpRequest.Status = status;

        _context.Notifications.Add(new Notification
        {
            UserProfileId = helpRequest.UserProfileId,
            Message = $"Your support request '{helpRequest.Title}' status changed to {status}."
        });

        await _context.SaveChangesAsync();

        TempData["Message"] = "Support request status updated.";
        return RedirectToAction(nameof(Details), new { id });
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

    private async Task<bool> CanViewHelpRequestAsync(HelpRequest helpRequest)
    {
        if (User.IsInRole("Admin")) return true;
        var profile = await GetCurrentProfileAsync();
        return CanViewHelpRequest(helpRequest, profile, false);
    }

    private async Task<bool> CanManageHelpRequestAsync(HelpRequest helpRequest)
    {
        if (User.IsInRole("Admin")) return true;
        var profile = await GetCurrentProfileAsync();
        return CanManageHelpRequest(helpRequest, profile, false);
    }

    private async Task<bool> CanUpdateHelpRequestStatusAsync(HelpRequest helpRequest)
    {
        if (User.IsInRole("Admin")) return true;
        var profile = await GetCurrentProfileAsync();
        return CanUpdateHelpRequestStatus(helpRequest, profile, false);
    }

    private static bool CanViewHelpRequest(HelpRequest helpRequest, UserProfile? currentProfile, bool isAdmin)
    {
        if (isAdmin) return true;
        if (currentProfile == null) return false;

        // Support Requests are for system/app issues.
        // Users can view only their own support requests; Admin can view all.
        return helpRequest.UserProfileId == currentProfile.Id;
    }

    private static bool CanManageHelpRequest(HelpRequest helpRequest, UserProfile? currentProfile, bool isAdmin)
    {
        if (isAdmin) return true;
        if (currentProfile == null) return false;

        // Only the request owner or Admin can edit/delete a support request.
        return helpRequest.UserProfileId == currentProfile.Id;
    }

    private static bool CanUpdateHelpRequestStatus(HelpRequest helpRequest, UserProfile? currentProfile, bool isAdmin)
    {
        // Support request status is handled by Admin/system support side only.
        return isAdmin;
    }
}
