using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;

namespace OCVMS.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var today = DateTime.Today;
            var featuredEvents = await _context.VolunteerEvents
                .Where(e => e.EventDate >= today)
                .OrderBy(e => e.EventDate)
                .ThenBy(e => e.EventTime)
                .Take(3)
                .ToListAsync();

            ViewBag.UserCount = await _context.UserProfiles.CountAsync();
            ViewBag.TotalEvents = await _context.VolunteerEvents.CountAsync();
            ViewBag.ActiveEvents = await _context.VolunteerEvents.CountAsync(e => e.EventDate >= today && e.Status != "Closed");

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

    public IActionResult Privacy() => View();

    public IActionResult About() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
