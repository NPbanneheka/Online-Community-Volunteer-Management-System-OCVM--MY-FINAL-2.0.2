using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.ViewModels;

namespace OCVMS.Controllers;

/// <summary>
/// පරිශීලකයාගේ ක්‍රියාකාරකම් සහ පද්ධතියේ දත්ත සාරාංශය පාලනය කරයි.
/// </summary>
[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // 1. දැනට ලොග් වී සිටින පරිශීලකයා ලබා ගැනීම
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        // 2. Dashboard එකට අවශ්‍ය සංඛ්‍යාලේඛන ගණනය කිරීම (Linq Queries)
        var vm = new DashboardViewModel
        {
            // පද්ධතියේ ඇති මුළු ඉවෙන්ට් ගණන
            TotalEvents = await _context.VolunteerEvents.CountAsync(),
            
            // මම (පරිශීලකයා) ලියාපදිංචි වී ඇති ඉවෙන්ට් ගණන
            MyRegistrations = await _context.EventRegistrations.CountAsync(x => x.UserId == user.Id),
            
            // Community එකේ ඇති මුළු පෝස්ට් ගණන
            CommunityPosts = await _context.CommunityPosts.CountAsync(),
            
            // මට ලැබී ඇති සාමාන්‍ය රේටින්ග් අගය (Average Score)
            AverageRating = await _context.UserRatings
                .Where(x => x.ToUserId == user.Id)
                .Select(x => (double?)x.Score)
                .AverageAsync() ?? 0
        };

        // 3. අමතර දත්ත ලබා ගැනීම (ViewBag හරහා)
        ViewBag.MyProfile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
        
        // ඉදිරියට එන ඉවෙන්ට් 5ක් (Upcoming Events)
        ViewBag.UpcomingEvents = await _context.VolunteerEvents
            .Where(x => x.EventDate >= DateTime.Now)
            .OrderBy(x => x.EventDate)
            .Take(5)
            .ToListAsync();

        // මෑතකදී දමන ලද පෝස්ට් 5ක් (Recent Community Posts)
        ViewBag.RecentPosts = await _context.CommunityPosts
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View(vm);
    }
}