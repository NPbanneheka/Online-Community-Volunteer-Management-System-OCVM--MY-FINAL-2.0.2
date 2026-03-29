using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;
using OCVMS.ViewModels;

namespace OCVMS.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> MyProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
        return View(profile);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id) ?? new UserProfile { UserId = user.Id, FullName = user.Email ?? "User" };
        return View(new ProfileEditViewModel
        {
            FullName = profile.FullName,
            PublicEmail = profile.PublicEmail,
            ContactNumber = profile.ContactNumber,
            Bio = profile.Bio,
            Skills = profile.Skills,
            Availability = profile.Availability,
            ProfileImageUrl = profile.ProfileImageUrl,
            OrganizationName = profile.OrganizationName
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileEditViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid) return View(model);

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (profile == null)
        {
            profile = new UserProfile { UserId = user.Id, RoleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Volunteer" };
            _context.UserProfiles.Add(profile);
        }

        profile.FullName = model.FullName;
        profile.PublicEmail = model.PublicEmail;
        profile.ContactNumber = model.ContactNumber;
        profile.Bio = model.Bio;
        profile.Skills = model.Skills;
        profile.Availability = model.Availability;
        profile.ProfileImageUrl = model.ProfileImageUrl;
        profile.OrganizationName = model.OrganizationName;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(MyProfile));
    }

    [AllowAnonymous]
    public async Task<IActionResult> ViewProfile(string id)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == id);
        if (profile == null) return NotFound();

        ViewBag.AverageRating = await _context.UserRatings.Where(x => x.ToUserId == id).Select(x => (double?)x.Score).AverageAsync() ?? 0;
        return View(profile);
    }
}
