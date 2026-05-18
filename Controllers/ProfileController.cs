// Handles profiles and admin-side user management: view/edit profile, verify, ban, unban, and delete users.
// Technology map:
// - ASP.NET Core MVC handles profile pages and admin actions.
// - ASP.NET Core Identity manages account-level data, roles, lockout, and deletion.
// - EF Core manages project data such as profiles, events, registrations, posts, ratings, and notifications.
// - IWebHostEnvironment is used for profile image upload paths.
// Connected files: ProfileEditViewModel, UserProfile model, Profile views, related project entities.

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
    private readonly ApplicationDbContext _context; // EF Core context connected to SQL Server tables.
    private readonly UserManager<IdentityUser> _userManager; // Identity service for user lookup, roles, and account operations.
    private readonly IWebHostEnvironment _environment; // Provides wwwroot paths for uploaded image files.

    public ProfileController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
    }

    // Shows the current user profile with personal and role-related information.

    public async Task<IActionResult> MyProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var profile = await GetPrimaryProfileForUserAsync(user.Id);
        return View(profile);
    }

    [HttpGet]
    // Edit page/action: GET loads existing data; POST validates ownership/role, updates fields, and saves changes.
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
                         IsVerified = roleName == "Admin"
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
    // Edit page/action: GET loads existing data; POST validates ownership/role, updates fields, and saves changes.
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
                IsVerified = roleName == "Admin"
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
        // If no new image is uploaded, keep the existing saved image path.
        // Do not trust the hidden ProfileImageUrl field because it can be changed from the browser.
        await _context.SaveChangesAsync();
        TempData["Message"] = "Profile updated successfully!";
        return RedirectToAction(nameof(MyProfile));
    }

    [AllowAnonymous]
    // Shows another user profile in read-only mode.
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

    // Main listing page: loads records, applies filters/search, prepares ViewBag data, then returns the view.

    public IActionResult Index()
    {
        return RedirectToAction(nameof(MyProfile));
    }

    [Authorize(Roles = "Admin")]
    // Admin user-management page: lists users and supports verification, ban/unban, and delete actions.
    public async Task<IActionResult> ManageUsers()
    {
        var adminProfilesToFix = await _context.UserProfiles
            .Where(p => p.RoleName == "Admin" && !p.IsVerified)
            .ToListAsync();

        if (adminProfilesToFix.Any())
        {
            foreach (var adminProfile in adminProfilesToFix)
            {
                adminProfile.IsVerified = true;
            }

            await _context.SaveChangesAsync();
        }

        var allProfiles = await _context.UserProfiles
            .AsNoTracking()
            .OrderBy(p => p.RoleName)
            .ThenBy(p => p.FullName)
            .ToListAsync();

        var profiles = allProfiles
            .GroupBy(p => p.UserId)
            .Select(g => g
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
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
    // Admin action: marks a selected user profile as verified.
    public async Task<IActionResult> VerifyUser(int id)
    {
        return await SetVerificationStatusAsync(id, true);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnverifyUser(int id)
    {
        return await SetVerificationStatusAsync(id, false);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TempBanUser(int id)
    {
        return await SetTemporaryBanStatusAsync(id, true);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    // Admin action: re-enables a previously banned user account.
    public async Task<IActionResult> UnbanUser(int id)
    {
        return await SetTemporaryBanStatusAsync(id, false);
    }

    private async Task<IActionResult> SetVerificationStatusAsync(int id, bool verified)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == id);
        if (profile == null) return NotFound();

        var user = await _userManager.FindByIdAsync(profile.UserId);
        if (user == null)
        {
        TempData["Message"] = "Identity account was not found for the selected user.";
            return RedirectToAction(nameof(ManageUsers));
        }

        if (string.Equals(profile.RoleName, "Admin", StringComparison.OrdinalIgnoreCase) || await _userManager.IsInRoleAsync(user, "Admin"))
        {
        TempData["Message"] = "Admin accounts are protected system accounts. Their verification status cannot be changed from User Management.";
            return RedirectToAction(nameof(ManageUsers));
        }

        var relatedProfiles = await _context.UserProfiles
            .Where(p => p.UserId == profile.UserId)
            .ToListAsync();

        foreach (var relatedProfile in relatedProfiles)
        {
            relatedProfile.IsVerified = verified;
        }

        await _context.SaveChangesAsync();

        TempData["Message"] = verified
            ? $"{profile.FullName} is now verified."
            : $"{profile.FullName} is now pending verification.";

        return RedirectToAction(nameof(ManageUsers));
    }

    private async Task<IActionResult> SetTemporaryBanStatusAsync(int id, bool ban)
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

        if (string.Equals(profile.RoleName, "Admin", StringComparison.OrdinalIgnoreCase) || await _userManager.IsInRoleAsync(user, "Admin"))
        {
        TempData["Message"] = "Admin accounts are protected and cannot be temporarily banned or unbanned from this page.";
            return RedirectToAction(nameof(ManageUsers));
        }

        user.LockoutEnabled = true;
        user.LockoutEnd = ban ? DateTimeOffset.UtcNow.AddDays(90) : null;

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
    // Admin action: removes a selected user and related records safely.
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

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
        TempData["Message"] = "Admin accounts are protected and cannot be deleted from this page.";
            return RedirectToAction(nameof(ManageUsers));
        }

        var deletedFullName = profile.FullName;
        var strategy = _context.Database.CreateExecutionStrategy();

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
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

                    var ratings = await _context.UserRatings
                        .Where(r => r.FromUserId == user.Id || r.ToUserId == user.Id || organizedEventIds.Contains(r.EventId))
                        .ToListAsync();
                    _context.UserRatings.RemoveRange(ratings);

                    var registrations = await _context.EventRegistrations
                        .Where(r => r.UserId == user.Id || organizedEventIds.Contains(r.VolunteerEventId))
                        .ToListAsync();
                    _context.EventRegistrations.RemoveRange(registrations);

                    var notifications = await _context.Notifications
                        .Where(n => profileIds.Contains(n.UserProfileId))
                        .ToListAsync();
                    _context.Notifications.RemoveRange(notifications);

                    var comments = await _context.PostComments
                        .Where(c => profileIds.Contains(c.UserProfileId) || postIds.Contains(c.CommunityPostId))
                        .ToListAsync();
                    _context.PostComments.RemoveRange(comments);

                    var posts = await _context.CommunityPosts
                        .Where(p => profileIds.Contains(p.UserProfileId))
                        .ToListAsync();
                    _context.CommunityPosts.RemoveRange(posts);

                    var helpRequests = await _context.HelpRequests
                        .Where(h => profileIds.Contains(h.UserProfileId))
                        .ToListAsync();
                    _context.HelpRequests.RemoveRange(helpRequests);

                    _context.VolunteerEvents.RemoveRange(organizedEvents);
                    _context.UserProfiles.RemoveRange(relatedProfiles);

                    await _context.SaveChangesAsync();

                    var deleteResult = await _userManager.DeleteAsync(user);
                    if (!deleteResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            "User account could not be deleted: " +
                            string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
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
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

        TempData["Message"] = $"{deletedFullName}'s account and all related data were deleted successfully.";
        }
        catch (Exception ex)
        {
        TempData["Message"] = ex.Message;
        }

        return RedirectToAction(nameof(ManageUsers));
    }

    private async Task<UserProfile?> GetPrimaryProfileForUserAsync(string userId)
    {
        return await _context.UserProfiles
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
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
