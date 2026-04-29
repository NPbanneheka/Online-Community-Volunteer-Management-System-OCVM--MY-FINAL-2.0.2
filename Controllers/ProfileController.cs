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
                DeleteLocalFile(profile.ProfileImageUrl, "profiles");
            }

            profile.ProfileImageUrl = "/uploads/profiles/" + uniqueFileName;
        }

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

        var profiles = allProfiles
            .GroupBy(p => p.UserId)
            .Select(g => g
                .OrderByDescending(p => p.IsVerified)
                .ThenByDescending(p => p.CreatedAt)
                .First())
            .OrderBy(p => p.RoleName)
            .ThenBy(p => p.FullName)
            .ToList();

        var userIds = profiles.Select(p => p.UserId).Distinct().ToList();
        var lockoutMap = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.LockoutEnd);

        ViewBag.LockoutMap = lockoutMap;
        ViewBag.CurrentAdminUserId = _userManager.GetUserId(User);

        return View(profiles);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(int id, bool isVerified)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == id);
        if (profile == null) return NotFound();

        var relatedProfiles = await _context.UserProfiles
            .Where(p => p.UserId == profile.UserId)
            .ToListAsync();

        foreach (var relatedProfile in relatedProfiles)
        {
            relatedProfile.IsVerified = isVerified;
        }

        await _context.SaveChangesAsync();

        TempData["Message"] = isVerified
            ? $"{profile.FullName} has been verified successfully."
            : $"{profile.FullName} has been marked as unverified / pending verification.";

        return RedirectToAction(nameof(ManageUsers));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTempBan(int id, bool ban)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == id);
        if (profile == null) return NotFound();

        var user = await _userManager.FindByIdAsync(profile.UserId);
        if (user == null)
        {
            TempData["Message"] = "Identity account was not found for the selected user.";
            return RedirectToAction(nameof(ManageUsers));
        }

        var currentUserId = _userManager.GetUserId(User);
        if (user.Id == currentUserId)
        {
            TempData["Message"] = "You cannot temporarily ban or unban your own currently logged-in admin account.";
            return RedirectToAction(nameof(ManageUsers));
        }

        user.LockoutEnabled = true;
        user.LockoutEnd = ban
            ? DateTimeOffset.UtcNow.AddDays(30)
            : null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            TempData["Message"] = "Could not update the temporary ban status: " +
                                  string.Join(", ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(ManageUsers));
        }

        TempData["Message"] = ban
            ? $"{profile.FullName} has been temporarily banned from logging in."
            : $"{profile.FullName} has been unbanned and can log in again.";

        return RedirectToAction(nameof(ManageUsers));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == id);
        if (profile == null) return NotFound();

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

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var relatedProfiles = await _context.UserProfiles
            .Where(p => p.UserId == user.Id)
            .ToListAsync();
        var profileIds = relatedProfiles.Select(p => p.Id).ToList();
        var profileImages = relatedProfiles
            .Select(p => p.ProfileImageUrl)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Cast<string>()
            .ToList();

        var organizedEvents = await _context.VolunteerEvents
            .Where(e => profileIds.Contains(e.OrganizerProfileId))
            .ToListAsync();
        var organizedEventIds = organizedEvents.Select(e => e.Id).ToList();
        var eventImages = organizedEvents
            .Select(e => e.ImageUrl)
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Cast<string>()
            .ToList();

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

        _context.VolunteerEvents.RemoveRange(organizedEvents);
        _context.UserProfiles.RemoveRange(relatedProfiles);

        await _context.SaveChangesAsync();

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            await transaction.RollbackAsync();
            TempData["Message"] = "User account could not be deleted: " +
                                  string.Join(", ", deleteResult.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(ManageUsers));
        }

        await transaction.CommitAsync();

        foreach (var profileImage in profileImages)
        {
            DeleteLocalFile(profileImage, "profiles");
        }

        foreach (var eventImage in eventImages)
        {
            DeleteLocalFile(eventImage, "events");
        }

        TempData["Message"] = $"{profile.FullName}'s account and all related data were deleted successfully.";
        return RedirectToAction(nameof(ManageUsers));
    }

    private async Task<UserProfile?> GetPrimaryProfileForUserAsync(string userId)
    {
        return await _context.UserProfiles
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsVerified)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    private void DeleteLocalFile(string? relativeUrl, string folderName)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
        {
            return;
        }

        var cleanedPath = relativeUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
        var fullPath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, cleanedPath));
        var allowedRoot = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "uploads", folderName));

        if (fullPath.StartsWith(allowedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}
