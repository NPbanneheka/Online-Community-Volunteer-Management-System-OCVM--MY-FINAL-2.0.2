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
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public NotificationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var profile = string.IsNullOrWhiteSpace(userId) ? null : await GetPrimaryProfileForUserAsync(userId);

        if (profile == null)
        {
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
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = _userManager.GetUserId(User);
        var profile = string.IsNullOrWhiteSpace(userId) ? null : await GetPrimaryProfileForUserAsync(userId);

        var notification = await _context.Notifications.FirstOrDefaultAsync(n =>
            n.Id == id && profile != null && n.UserProfileId == profile.Id);

        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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

    private async Task<UserProfile?> GetPrimaryProfileForUserAsync(string userId)
    {
        return await _context.UserProfiles
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsVerified)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
