using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.ViewModels;

namespace OCVMS.Controllers;

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
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var vm = new DashboardViewModel
        {
            TotalEvents = await _context.VolunteerEvents.CountAsync(),
            MyRegistrations = await _context.EventRegistrations.CountAsync(x => x.UserId == user.Id),
            CommunityPosts = await _context.CommunityPosts.CountAsync(),
            AverageRating = await _context.UserRatings.Where(x => x.ToUserId == user.Id).Select(x => (double?)x.Score).AverageAsync() ?? 0
        };

        ViewBag.MyProfile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
        ViewBag.UpcomingEvents = await _context.VolunteerEvents.OrderBy(x => x.EventDate).Take(5).ToListAsync();
        ViewBag.RecentPosts = await _context.CommunityPosts.OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync();

        return View(vm);
    }
}
