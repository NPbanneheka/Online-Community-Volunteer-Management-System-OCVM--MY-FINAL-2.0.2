// Builds the role-aware dashboard summary shown after login.
// Technology map:
// - ASP.NET Core MVC prepares dashboard data for Razor views.
// - EF Core counts users, events, registrations, help requests, and notifications.
// - ASP.NET Identity identifies the current user and role.
// Connected files: DashboardViewModel, UserProfile, VolunteerEvent, EventRegistration, HelpRequest.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;
using OCVMS.ViewModels;

namespace OCVMS.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private const string ApplicationRatingTargetId = "__APPLICATION__"; // Special rating target used when rating the whole platform instead of a single event.
    private const int ApplicationRatingEventId = 0; // EventId placeholder for platform-level ratings.

    private readonly ApplicationDbContext _context; // EF Core context connected to SQL Server tables.
    private readonly UserManager<IdentityUser> _userManager; // Identity service for user lookup, roles, and account operations.

    public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Main listing page: loads records, applies filters/search, prepares ViewBag data, then returns the view.

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var profile = await GetPrimaryProfileForUserAsync(user.Id);

        IQueryable<UserRating> ratingsQuery;
        string ratingTitle;
        string ratingEmptyText;

        if (User.IsInRole("Admin"))
        {
            ratingsQuery = _context.UserRatings.Where(x => x.EventId == ApplicationRatingEventId && x.ToUserId == ApplicationRatingTargetId);
            ratingTitle = "Platform Rating";
            ratingEmptyText = "No platform ratings yet";
        }
        else
        {
            ratingsQuery = _context.UserRatings.Where(x => x.ToUserId == user.Id);
            ratingTitle = "Average Rating";
            ratingEmptyText = "No ratings yet";
        }

        var vm = new DashboardViewModel
        {
            TotalEvents = await _context.VolunteerEvents.CountAsync(),
            ActiveEvents = await _context.VolunteerEvents.CountAsync(x => x.Status != "Closed" && x.Status != "Completed" && x.Status != "Cancelled"),
            MyRegistrations = await _context.EventRegistrations.CountAsync(x => x.UserId == user.Id),
            CommunityPosts = await _context.CommunityPosts.CountAsync(),
            CommunityHelpPosts = await _context.CommunityPosts.CountAsync(x => x.PostType == "Help"),
            AverageRating = await ratingsQuery.Select(x => (double?)x.Score).AverageAsync() ?? 0,
            RatingCount = await ratingsQuery.CountAsync()
        };
        ViewBag.MyProfile = profile;
        ViewBag.RatingTitle = ratingTitle;
        ViewBag.RatingEmptyText = ratingEmptyText;

        ViewBag.UpcomingEvents = await _context.VolunteerEvents
            .Where(x => x.EventDate >= DateTime.Today && x.Status != "Closed" && x.Status != "Completed" && x.Status != "Cancelled")
            .OrderBy(x => x.EventDate)
            .Take(5)
            .ToListAsync();

        ViewBag.RecentPosts = await _context.CommunityPosts
            .Include(p => p.User)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View(vm);
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
