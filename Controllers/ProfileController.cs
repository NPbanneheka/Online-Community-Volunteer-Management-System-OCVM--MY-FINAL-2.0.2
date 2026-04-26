using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;
using OCVMS.ViewModels;
using System.IO;

namespace OCVMS.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
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

        var roles = await _userManager.GetRolesAsync(user);
        var roleName = roles.FirstOrDefault() ?? "Volunteer";

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id)
                     ?? new UserProfile
                     {
                         UserId = user.Id,
                         FullName = user.Email ?? "User",
                         RoleName = roleName
                     };

        return View(new ProfileEditViewModel
        {
            Id = profile.Id,
            UserId = profile.UserId,
            RoleName = string.IsNullOrWhiteSpace(profile.RoleName) ? roleName : profile.RoleName,
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

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (profile == null)
        {
            profile = new UserProfile
            {
                UserId = user.Id,
                RoleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Volunteer"
            };
            _context.UserProfiles.Add(profile);
        }

        ModelState.Remove("ProfileImageUrl");
        ModelState.Remove("ProfileImageFile");
        ModelState.Remove("ProfileImage");
        ModelState.Remove("UserId");
        ModelState.Remove("RoleName");
        ModelState.Remove("Id");

        var effectiveRole = string.IsNullOrWhiteSpace(profile.RoleName)
            ? ((await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Volunteer")
            : profile.RoleName;

        if (effectiveRole == "Organizer" && string.IsNullOrWhiteSpace(model.OrganizationName))
        {
            ModelState.AddModelError(nameof(model.OrganizationName), "Organization name is required for organizer profiles.");
        }

        if (!ModelState.IsValid)
        {
            model.ProfileImageUrl = profile.ProfileImageUrl;
            model.RoleName = effectiveRole;
            return View(model);
        }

        profile.FullName = model.FullName.Trim();
        profile.PublicEmail = model.PublicEmail;
        profile.ContactNumber = model.ContactNumber;
        profile.Bio = model.Bio;
        profile.Skills = model.Skills;
        profile.Availability = model.Availability;
        profile.OrganizationName = model.OrganizationName?.Trim();

        var uploadedFile = Request.Form.Files.FirstOrDefault();

        if (uploadedFile != null && uploadedFile.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("", "Only JPG, JPEG, PNG, and WEBP files are allowed.");
                model.ProfileImageUrl = profile.ProfileImageUrl;
                model.RoleName = effectiveRole;
                return View(model);
            }

            if (uploadedFile.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("", "Image size must be less than 2MB.");
                model.ProfileImageUrl = profile.ProfileImageUrl;
                model.RoleName = effectiveRole;
                return View(model);
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(stream);
            }

            if (!string.IsNullOrEmpty(profile.ProfileImageUrl))
            {
                var oldRelativePath = profile.ProfileImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
                var oldFullPath = Path.Combine(_environment.WebRootPath, oldRelativePath);
                var profileUploadRoot = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                if (oldFullPath.StartsWith(profileUploadRoot) && System.IO.File.Exists(oldFullPath))
                {
                    System.IO.File.Delete(oldFullPath);
                }
            }

            profile.ProfileImageUrl = "/uploads/profiles/" + uniqueFileName;
        }
        else
        {
            profile.ProfileImageUrl = model.ProfileImageUrl;
        }

        await _context.SaveChangesAsync();
        TempData["Message"] = "Profile updated successfully!";
        return RedirectToAction(nameof(MyProfile));
    }

    [AllowAnonymous]
    public async Task<IActionResult> ViewProfile(string id)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == id);
        if (profile == null) return NotFound();

        ViewBag.AverageRating = await _context.UserRatings
            .Where(x => x.ToUserId == id)
            .Select(x => (double?)x.Score)
            .AverageAsync() ?? 0;

        return View(profile);
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            profile = new UserProfile
            {
                UserId = userId!,
                FullName = User.Identity?.Name ?? "User",
                RoleName = User.IsInRole("Organizer") ? "Organizer" : "Volunteer"
            };
            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }
        return View(profile);
    }
}
