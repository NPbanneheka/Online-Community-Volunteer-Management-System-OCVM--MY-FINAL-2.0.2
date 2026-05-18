// Controllers/DashboardController.cs
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
using OCVMS.ViewModels;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Controllers;

[Authorize]
// Controller class: methods inside this class respond to user actions from the browser.
public class DashboardController : Controller
{
    private const string ApplicationRatingTargetId = "__APPLICATION__";
    private const int ApplicationRatingEventId = 0;

    // _context connects this class to SQL Server through Entity Framework Core and ApplicationDbContext.
    private readonly ApplicationDbContext _context;
    // _userManager works with ASP.NET Identity users, including lookup, creation, roles, and passwords.
    private readonly UserManager<IdentityUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Loads the default page or list view for this controller.

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        // Handles missing data safely before continuing with the requested operation.
        // Redirects the browser to another MVC action after the current operation is complete.
        if (user == null) return RedirectToAction("Login", "Account");

        var profile = await GetPrimaryProfileForUserAsync(user.Id);

        IQueryable<UserRating> ratingsQuery;
        string ratingTitle;
        string ratingEmptyText;

        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
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
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(p => p.User)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .ToListAsync();

        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(vm);
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
