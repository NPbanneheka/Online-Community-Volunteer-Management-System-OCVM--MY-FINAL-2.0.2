// ================================================================
// VIVA COMMENTED VERSION - Controllers/NotificationsController.cs
// Purpose: Shows and manages user notifications.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;

namespace OCVMS.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    // Dependencies injected through constructor for database, identity, hosting, or logging work.
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public NotificationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Main listing page: loads records, applies filters/search, prepares ViewBag data, then returns the view.

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var profile = string.IsNullOrWhiteSpace(userId) ? null : await GetPrimaryProfileForUserAsync(userId);

        if (profile == null)
        {
            // TempData message is shown once after redirect.
            TempData["Message"] = "Please complete your profile to view notifications.";
            return RedirectToAction("Edit", "Profile");
        }

        var notifications = await _context.Notifications
            .Where(n => n.UserProfileId == profile.Id)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Controller action: handles a request, performs validation/business logic, and returns a response/view.
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = _userManager.GetUserId(User);
        var profile = string.IsNullOrWhiteSpace(userId) ? null : await GetPrimaryProfileForUserAsync(userId);

        var notification = await _context.Notifications.FirstOrDefaultAsync(n =>
            n.Id == id && profile != null && n.UserProfileId == profile.Id);

        if (notification != null)
        {
            notification.IsRead = true;
            // Save all pending database changes.
        await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Controller action: handles a request, performs validation/business logic, and returns a response/view.
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = _userManager.GetUserId(User);
        var profile = string.IsNullOrWhiteSpace(userId) ? null : await GetPrimaryProfileForUserAsync(userId);

        if (profile != null)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserProfileId == profile.Id && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // Helper method: keeps repeated controller logic in one reusable place.

    private async Task<UserProfile?> GetPrimaryProfileForUserAsync(string userId)
    {
        return await _context.UserProfiles
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsVerified)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
