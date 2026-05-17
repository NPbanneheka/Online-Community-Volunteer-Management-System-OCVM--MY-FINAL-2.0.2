// ================================================================
// VIVA COMMENTED VERSION - Controllers/HomeController.cs
// Purpose: Handles public home, about, privacy, and error pages.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;

namespace OCVMS.Controllers;

public class HomeController : Controller
{
    private const string ApplicationRatingTargetId = "__APPLICATION__";
    private const int ApplicationRatingEventId = 0;
    // Dependencies injected through constructor for database, identity, hosting, or logging work.

    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    // Main listing page: loads records, applies filters/search, prepares ViewBag data, then returns the view.

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

            // ViewBag passes small extra values to the Razor view.
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

            return View(featuredEvents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Home page data loading failed.");
            ViewBag.UserCount = 0;
            ViewBag.TotalEvents = 0;
            ViewBag.ActiveEvents = 0;
            return View(new List<VolunteerEvent>());
        }
    }

    // Controller action: handles a request, performs validation/business logic, and returns a response/view.

    public IActionResult Privacy() => View();

    // Controller action: handles a request, performs validation/business logic, and returns a response/view.

    public async Task<IActionResult> About()
    {
        var appRatingsQuery = _context.UserRatings.Where(r => r.EventId == ApplicationRatingEventId && r.ToUserId == ApplicationRatingTargetId);
        ViewBag.AppAverageRating = await appRatingsQuery.Select(r => (double?)r.Score).AverageAsync() ?? 0;
        ViewBag.AppRatingCount = await appRatingsQuery.CountAsync();

        var currentUserId = _userManager.GetUserId(User);
        ViewBag.MyAppRating = string.IsNullOrWhiteSpace(currentUserId)
            ? null
            : await _context.UserRatings.FirstOrDefaultAsync(r => r.EventId == ApplicationRatingEventId && r.ToUserId == ApplicationRatingTargetId && r.FromUserId == currentUserId);

        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    // Controller action: handles a request, performs validation/business logic, and returns a response/view.
    public async Task<IActionResult> RateApplication(int score, string? reviewText)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction(nameof(About));

        if (score < 1 || score > 5)
        {
            // TempData message is shown once after redirect.
            TempData["Message"] = "Please select a rating between 1 and 5.";
            return RedirectToAction(nameof(About));
        }

        var rating = await _context.UserRatings.FirstOrDefaultAsync(r =>
            r.EventId == ApplicationRatingEventId && r.ToUserId == ApplicationRatingTargetId && r.FromUserId == userId);

        if (rating == null)
        {
            rating = new UserRating
            {
                EventId = ApplicationRatingEventId,
                FromUserId = userId,
                ToUserId = ApplicationRatingTargetId
            };
            _context.UserRatings.Add(rating);
        }

        rating.Score = score;
        rating.ReviewText = reviewText;
        // Save all pending database changes.
        await _context.SaveChangesAsync();

        TempData["Message"] = "Thank you for rating the platform.";
        return RedirectToAction(nameof(About));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // Controller action: handles a request, performs validation/business logic, and returns a response/view.
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
