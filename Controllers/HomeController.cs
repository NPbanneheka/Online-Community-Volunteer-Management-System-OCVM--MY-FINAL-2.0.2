// Controllers/HomeController.cs
// This MVC controller file that receives browser requests and coordinates models, services, and views.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
using System.Diagnostics;
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

// Controller class: methods inside this class respond to user actions from the browser.
public class HomeController : Controller
{
    private const string ApplicationRatingTargetId = "__APPLICATION__";
    private const int ApplicationRatingEventId = 0;

    // _logger records diagnostic information for troubleshooting application behavior.
    private readonly ILogger<HomeController> _logger;
    // _context connects this class to SQL Server through Entity Framework Core and ApplicationDbContext.
    private readonly ApplicationDbContext _context;
    // _userManager works with ASP.NET Identity users, including lookup, creation, roles, and passwords.
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    // Loads the default page or list view for this controller.

    public async Task<IActionResult> Index()
    {
        try
        {
            var today = DateTime.Today;
            var featuredEvents = await _context.VolunteerEvents
                .Where(e => e.EventDate >= today && e.Status != "Closed" && e.Status != "Completed" && e.Status != "Cancelled")
                .OrderBy(e => e.EventDate)
                .ThenBy(e => e.EventTime)
                .Take(3)
                .ToListAsync();

            ViewBag.UserCount = await _context.UserProfiles
                .Select(u => u.UserId)
                .Distinct()
                .CountAsync();
            ViewBag.TotalEvents = await _context.VolunteerEvents.CountAsync();
            ViewBag.ActiveEvents = await _context.VolunteerEvents.CountAsync(e =>
                e.EventDate >= today &&
                e.RegistrationOpenDate <= today &&
                (!e.RegistrationClosingDate.HasValue || e.RegistrationClosingDate.Value.Date >= today) &&
                e.Status != "Closed" && e.Status != "Completed" && e.Status != "Cancelled");

            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(featuredEvents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Home page data loading failed.");
            ViewBag.UserCount = 0;
            ViewBag.TotalEvents = 0;
            ViewBag.ActiveEvents = 0;
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(new List<VolunteerEvent>());
        }
    }

    // Handles the Privacy request using MVC action logic and returns the appropriate response.

    public IActionResult Privacy() => View();

    // Handles the About request using MVC action logic and returns the appropriate response.

    public async Task<IActionResult> About()
    {
        var appRatingsQuery = _context.UserRatings.Where(r => r.EventId == ApplicationRatingEventId && r.ToUserId == ApplicationRatingTargetId);
        ViewBag.AppAverageRating = await appRatingsQuery.Select(r => (double?)r.Score).AverageAsync() ?? 0;
        ViewBag.AppRatingCount = await appRatingsQuery.CountAsync();

        // Reads the currently logged-in Identity user ID from the authentication cookie.
        var currentUserId = _userManager.GetUserId(User);
        ViewBag.MyAppRating = string.IsNullOrWhiteSpace(currentUserId)
            ? null
            : await _context.UserRatings.FirstOrDefaultAsync(r => r.EventId == ApplicationRatingEventId && r.ToUserId == ApplicationRatingTargetId && r.FromUserId == currentUserId);

        // Sends data to a Razor view so the page can be rendered in the browser.
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    // Handles the RateApplication request using MVC action logic and returns the appropriate response.
    public async Task<IActionResult> RateApplication(int score, string? reviewText)
    {
        // Reads the currently logged-in Identity user ID from the authentication cookie.
        var userId = _userManager.GetUserId(User);
        // Redirects the browser to another MVC action after the current operation is complete.
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction(nameof(About));

        if (score < 1 || score > 5)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Please select a rating between 1 and 5.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(About));
        }

        // Retrieves a single matching database record asynchronously; returns null when not found.
        var rating = await _context.UserRatings.FirstOrDefaultAsync(r =>
            r.EventId == ApplicationRatingEventId && r.ToUserId == ApplicationRatingTargetId && r.FromUserId == userId);

        // Handles missing data safely before continuing with the requested operation.
        if (rating == null)
        {
            rating = new UserRating
            {
                EventId = ApplicationRatingEventId,
                FromUserId = userId,
                ToUserId = ApplicationRatingTargetId
            };
            // Adds a new entity to EF Core change tracking so it can be inserted into the database.
            _context.UserRatings.Add(rating);
        }

        rating.Score = score;
        rating.ReviewText = reviewText;
        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Thank you for rating the platform.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(About));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // Handles the Error request using MVC action logic and returns the appropriate response.
    public IActionResult Error()
    {
        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
