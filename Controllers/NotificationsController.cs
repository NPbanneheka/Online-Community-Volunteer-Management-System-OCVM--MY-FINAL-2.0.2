// Controllers/NotificationsController.cs
// This MVC controller file that receives browser requests and coordinates models, services, and views.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// ASP.NET Core authorization attributes such as [Authorize] and [AllowAnonymous].
using Microsoft.AspNetCore.Authorization;
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
public class NotificationsController : Controller
{
    // _context connects this class to SQL Server through Entity Framework Core and ApplicationDbContext.
    private readonly ApplicationDbContext _context;
    // _userManager works with ASP.NET Identity users, including lookup, creation, roles, and passwords.
    private readonly UserManager<IdentityUser> _userManager;

    public NotificationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Loads the default page or list view for this controller.

    public async Task<IActionResult> Index()
    {
        // Reads the currently logged-in Identity user ID from the authentication cookie.
        var userId = _userManager.GetUserId(User);
        var profile = string.IsNullOrWhiteSpace(userId) ? null : await GetPrimaryProfileForUserAsync(userId);

        // Handles missing data safely before continuing with the requested operation.
        if (profile == null)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Please complete your profile to view notifications.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction("Edit", "Profile");
        }

        var notifications = await _context.Notifications
            .Where(n => n.UserProfileId == profile.Id)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Updates a notification so it no longer appears as unread.
    public async Task<IActionResult> MarkAsRead(int id)
    {
        // Reads the currently logged-in Identity user ID from the authentication cookie.
        var userId = _userManager.GetUserId(User);
        var profile = string.IsNullOrWhiteSpace(userId) ? null : await GetPrimaryProfileForUserAsync(userId);

        // Retrieves a single matching database record asynchronously; returns null when not found.
        var notification = await _context.Notifications.FirstOrDefaultAsync(n =>
            n.Id == id && profile != null && n.UserProfileId == profile.Id);

        if (notification != null)
        {
            notification.IsRead = true;
            // Commits all pending EF Core changes to the SQL Server database.
            await _context.SaveChangesAsync();
        }

        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Handles the MarkAllAsRead request using MVC action logic and returns the appropriate response.
    public async Task<IActionResult> MarkAllAsRead()
    {
        // Reads the currently logged-in Identity user ID from the authentication cookie.
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

            // Commits all pending EF Core changes to the SQL Server database.
            await _context.SaveChangesAsync();
        }

        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(Index));
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
}
