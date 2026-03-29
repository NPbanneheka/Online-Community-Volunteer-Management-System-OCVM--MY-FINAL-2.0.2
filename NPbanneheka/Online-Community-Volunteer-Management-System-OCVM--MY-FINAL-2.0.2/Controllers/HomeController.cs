using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;

namespace OCVMS.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.FeaturedEvents = await _context.VolunteerEvents
            .OrderBy(x => x.EventDate)
            .Take(3)
            .ToListAsync();

        ViewBag.Stats = new
        {
            Users = await _context.UserProfiles.CountAsync(),
            Events = await _context.VolunteerEvents.CountAsync(),
            Registrations = await _context.EventRegistrations.CountAsync()
        };

        return View();
    }

    public IActionResult About() => View();
    public IActionResult Error() => View();
}
