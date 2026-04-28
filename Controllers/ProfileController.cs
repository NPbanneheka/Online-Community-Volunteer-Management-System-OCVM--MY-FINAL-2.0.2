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

        var profile = await GetPrimaryProfileForUserAsync(user.Id);
        return View(profile);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var roles = await _userManager.GetRolesAsync(user);
        var roleName = roles.FirstOrDefault() ?? "Volunteer";

        var profile = await GetPrimaryProfileForUserAsync(user.Id)
                     ?? new UserProfile
                     {
                         UserId = user.Id,
                         FullName = user.Email ?? "User",
                         PublicEmail = user.Email,
                         RoleName = roleName,
                         IsVerified = roleName != "Organizer"
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

        var profile = await GetPrimaryProfileForUserAsync(user.Id);
        if (profile == null)
        {
            var roleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Volunteer";
            profile = new UserProfile
            {
                UserId = user.Id,
                RoleName = roleName,
                IsVerified = roleName != "Organizer"
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
                var oldFullPath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, oldRelativePath));
                var profileUploadRoot = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "uploads", "profiles"));
                if (oldFullPath.StartsWith(profileUploadRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(oldFullPath))
                {
                    System.IO.File.Delete(oldFullPath);
                }
            }

            profile.ProfileImageUrl = "/uploads/profiles/" + uniqueFileName;
        }
        // If no new image is uploaded, keep the existing saved image path.
        // Do not trust the hidden ProfileImageUrl field because it can be changed from the browser.

        await _context.SaveChangesAsync();
        TempData["Message"] = "Profile updated successfully!";
        return RedirectToAction(nameof(MyProfile));
    }

    [AllowAnonymous]
    public async Task<IActionResult> ViewProfile(string id)
    {
        var profile = await GetPrimaryProfileForUserAsync(id);
        if (profile == null) return NotFound();

        ViewBag.AverageRating = await _context.UserRatings
            .Where(x => x.ToUserId == id)
            .Select(x => (double?)x.Score)
            .AverageAsync() ?? 0;

        return View(profile);
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(MyProfile));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ManageUsers()
    {
        var allProfiles = await _context.UserProfiles
            .OrderBy(p => p.RoleName)
            .ThenBy(p => p.FullName)
            .ToListAsync();

        // If older test runs created duplicate profiles for the same Identity user,
        // show only the most reliable profile row in the admin list.
        var profiles = allProfiles
            .GroupBy(p => p.UserId)
            .Select(g => g
                .OrderByDescending(p => p.IsVerified)
                .ThenByDescending(p => p.CreatedAt)
                .First())
            .OrderBy(p => p.RoleName)
            .ThenBy(p => p.FullName)
            .ToList();

        return View(profiles);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(int id, bool isVerified)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == id);
        if (profile == null) return NotFound();

        // Update all profile rows for the same Identity user. This also fixes older duplicate-profile data.
        var relatedProfiles = await _context.UserProfiles
            .Where(p => p.UserId == profile.UserId)
            .ToListAsync();

        foreach (var relatedProfile in relatedProfiles)
        {
            relatedProfile.IsVerified = isVerified;
        }

        await _context.SaveChangesAsync();

        TempData["Message"] = isVerified
            ? $"{profile.FullName} has been marked as verified."
            : $"{profile.FullName} has been marked as pending verification.";

        return RedirectToAction(nameof(ManageUsers));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == id);
        if (profile == null)
        {
            TempData["Message"] = "User profile was not found.";
            return RedirectToAction(nameof(ManageUsers));
        }

        var user = await _userManager.FindByIdAsync(profile.UserId);
        if (user == null)
        {
            TempData["Message"] = "Identity account was not found. Please check this user manually.";
            return RedirectToAction(nameof(ManageUsers));
        }

        var currentUserId = _userManager.GetUserId(User);
        if (user.Id == currentUserId)
        {
            TempData["Message"] = "You cannot delete your own admin account while logged in.";
            return RedirectToAction(nameof(ManageUsers));
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["Message"] = "Admin accounts are protected and cannot be deleted from this page.";
            return RedirectToAction(nameof(ManageUsers));
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var profileIds = await _context.UserProfiles
                .Where(p => p.UserId == user.Id)
                .Select(p => p.Id)
                .ToListAsync();

            var organizedEventIds = await _context.VolunteerEvents
                .Where(e => profileIds.Contains(e.OrganizerProfileId))
                .Select(e => e.Id)
                .ToListAsync();

            var postIds = await _context.CommunityPosts
                .Where(p => profileIds.Contains(p.UserProfileId))
                .Select(p => p.Id)
                .ToListAsync();

            _context.UserRatings.RemoveRange(await _context.UserRatings
                .Where(r => r.FromUserId == user.Id || r.ToUserId == user.Id || organizedEventIds.Contains(r.EventId))
                .ToListAsync());

            _context.EventRegistrations.RemoveRange(await _context.EventRegistrations
                .Where(r => r.UserId == user.Id || organizedEventIds.Contains(r.VolunteerEventId))
                .ToListAsync());

            _context.Notifications.RemoveRange(await _context.Notifications
                .Where(n => profileIds.Contains(n.UserProfileId))
                .ToListAsync());

            _context.PostComments.RemoveRange(await _context.PostComments
                .Where(c => profileIds.Contains(c.UserProfileId) || postIds.Contains(c.CommunityPostId))
                .ToListAsync());

            _context.CommunityPosts.RemoveRange(await _context.CommunityPosts
                .Where(p => profileIds.Contains(p.UserProfileId))
                .ToListAsync());

            _context.HelpRequests.RemoveRange(await _context.HelpRequests
                .Where(h => profileIds.Contains(h.UserProfileId))
                .ToListAsync());

            _context.VolunteerEvents.RemoveRange(await _context.VolunteerEvents
                .Where(e => organizedEventIds.Contains(e.Id))
                .ToListAsync());

            _context.UserProfiles.RemoveRange(await _context.UserProfiles
                .Where(p => profileIds.Contains(p.Id))
                .ToListAsync());

            await _context.SaveChangesAsync();

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, roles);
            }

            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                await transaction.RollbackAsync();
                TempData["Message"] = "User account could not be deleted: " +
                                      string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(ManageUsers));
            }

            await transaction.CommitAsync();

            TempData["Message"] = $"{profile.FullName}'s account and all related data were deleted successfully.";
            return RedirectToAction(nameof(ManageUsers));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["Message"] = "User could not be deleted. Error: " + ex.Message;
            return RedirectToAction(nameof(ManageUsers));
        }
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
