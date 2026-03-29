using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;

namespace OCVMS.Controllers;

public class EventsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public EventsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var events = await _context.VolunteerEvents.OrderBy(x => x.EventDate).ToListAsync();
        return View(events);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var item = await _context.VolunteerEvents.FindAsync(id);
        if (item == null) return NotFound();
        ViewBag.Registrations = await _context.EventRegistrations.CountAsync(x => x.VolunteerEventId == id);
        return View(item);
    }

    [Authorize(Roles = "Organizer,Admin")]
    public IActionResult Create() => View(new VolunteerEvent { EventDate = DateTime.Today.AddDays(7), Capacity = 50 });

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VolunteerEvent model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _userManager.GetUserAsync(User);
        model.OrganizerId = user?.Id ?? string.Empty;
        _context.VolunteerEvents.Add(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var exists = await _context.EventRegistrations.AnyAsync(x => x.VolunteerEventId == id && x.UserId == user.Id);
        if (!exists)
        {
            _context.EventRegistrations.Add(new EventRegistration { VolunteerEventId = id, UserId = user.Id });
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Organizer,Admin")]
    public async Task<IActionResult> MyEvents()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        var items = await _context.VolunteerEvents.Where(x => x.OrganizerId == user.Id).OrderByDescending(x => x.CreatedAt).ToListAsync();
        return View(items);
    }
}
