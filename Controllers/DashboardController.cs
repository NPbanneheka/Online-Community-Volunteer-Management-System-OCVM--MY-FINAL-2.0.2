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
    private const string ApplicationRatingTargetId = "__APPLICATION__";
    private const int ApplicationRatingEventId = 0;

    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var profile = await GetPrimaryProfileForUserAsync(user.Id);

<<<<<<< HEAD
        IQueryable<UserRating> ratingsQuery = _context.UserRatings.Where(x => x.ToUserId == user.Id);
        string ratingLabel = "Average Rating";
        string ratingSubText = "Based on your received rating(s)";

        if (User.IsInRole("Admin"))
        {
            ratingsQuery = _context.UserRatings;
            ratingLabel = "System Rating";
            ratingSubText = "Based on all event rating(s)";
        }
        else if (profile != null &&
                 string.Equals(profile.RoleName, "Organizer", StringComparison.OrdinalIgnoreCase) &&
                 !string.IsNullOrWhiteSpace(profile.OrganizationName))
        {
            var organizationName = profile.OrganizationName.Trim().ToLower();

            var organizationEventIds = await _context.VolunteerEvents
                .Include(e => e.OrganizerProfile)
                .Where(e => e.OrganizerProfile != null &&
                            e.OrganizerProfile.OrganizationName != null &&
                            e.OrganizerProfile.OrganizationName.Trim().ToLower() == organizationName)
                .Select(e => e.Id)
                .ToListAsync();

            ratingsQuery = _context.UserRatings
                .Where(r => organizationEventIds.Contains(r.EventId));

            ratingLabel = "Organization Rating";
            ratingSubText = $"Based on {profile.OrganizationName.Trim()} event rating(s)";
=======
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
>>>>>>> d6771abc44ca293a34d2889517c254d6fd455ff6
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
<<<<<<< HEAD
        ViewBag.RatingLabel = ratingLabel;
        ViewBag.RatingSubText = ratingSubText;
=======
        ViewBag.RatingTitle = ratingTitle;
        ViewBag.RatingEmptyText = ratingEmptyText;
>>>>>>> d6771abc44ca293a34d2889517c254d6fd455ff6

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
