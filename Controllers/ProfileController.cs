// Controllers/ProfileController.cs
// This MVC controller file that receives browser requests and coordinates models, services, and views.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// ASP.NET Core authorization attributes such as [Authorize] and [AllowAnonymous].
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
// ASP.NET Core Identity services for users, roles, passwords, sign-in sessions, and lockout/ban behavior.
using Microsoft.AspNetCore.Identity;
// ASP.NET Core MVC base classes and action results used by controllers.
using Microsoft.AspNetCore.Mvc;
// Entity Framework Core features such as Include(), Where(), ToListAsync(), and database queries.
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;
using OCVMS.ViewModels;
using System.IO;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Controllers;

[Authorize]
// Controller class: methods inside this class respond to user actions from the browser.
public class ProfileController : Controller
{
    // _context connects this class to SQL Server through Entity Framework Core and ApplicationDbContext.
    private readonly ApplicationDbContext _context;
    // _userManager works with ASP.NET Identity users, including lookup, creation, roles, and passwords.
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

    // Displays the current user profile and related profile information.

    public async Task<IActionResult> MyProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        // Handles missing data safely before continuing with the requested operation.
        // Redirects the browser to another MVC action after the current operation is complete.
        if (user == null) return RedirectToAction("Login", "Account");

        var profile = await GetPrimaryProfileForUserAsync(user.Id);
        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(profile);
    }

    [HttpGet]
    // Displays or processes the form used to update an existing record.
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        // Handles missing data safely before continuing with the requested operation.
        // Redirects the browser to another MVC action after the current operation is complete.
        if (user == null) return RedirectToAction("Login", "Account");

        var roles = await _userManager.GetRolesAsync(user);
        var roleName = roles.FirstOrDefault() ?? "Volunteer";

        var profile = await GetPrimaryProfileForUserAsync(user.Id)
                     // Creates an OCVMS profile record connected to an Identity user account.
                     ?? new UserProfile
                     {
                         UserId = user.Id,
                         FullName = user.Email ?? "User",
                         PublicEmail = user.Email,
                         RoleName = roleName,
                         IsVerified = roleName == "Admin"
                     };

        // Sends data to a Razor view so the page can be rendered in the browser.
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
    // Displays or processes the form used to update an existing record.
    public async Task<IActionResult> Edit(ProfileEditViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        // Handles missing data safely before continuing with the requested operation.
        // Redirects the browser to another MVC action after the current operation is complete.
        if (user == null) return RedirectToAction("Login", "Account");

        var profile = await GetPrimaryProfileForUserAsync(user.Id);
        // Handles missing data safely before continuing with the requested operation.
        if (profile == null)
        {
            var roleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Volunteer";
            // Creates an OCVMS profile record connected to an Identity user account.
            profile = new UserProfile
            {
                UserId = user.Id,
                RoleName = roleName,
                IsVerified = roleName == "Admin"
            };
            // Adds a new entity to EF Core change tracking so it can be inserted into the database.
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

        // Stops processing when form validation fails and returns the same view with validation messages.
        if (!ModelState.IsValid)
        {
            model.ProfileImageUrl = profile.ProfileImageUrl;
            model.RoleName = effectiveRole;
            // Sends data to a Razor view so the page can be rendered in the browser.
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
                // Sends data to a Razor view so the page can be rendered in the browser.
                return View(model);
            }

            if (uploadedFile.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("", "Image size must be less than 2MB.");
                model.ProfileImageUrl = profile.ProfileImageUrl;
                model.RoleName = effectiveRole;
                // Sends data to a Razor view so the page can be rendered in the browser.
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

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();
        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Profile updated successfully!";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(MyProfile));
    }

    [AllowAnonymous]
    // Displays another user profile in read-only mode.
    public async Task<IActionResult> ViewProfile(string id)
    {
        var profile = await GetPrimaryProfileForUserAsync(id);
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (profile == null) return NotFound();

        ViewBag.AverageRating = await _context.UserRatings
            .Where(x => x.ToUserId == id)
            .Select(x => (double?)x.Score)
            .AverageAsync() ?? 0;

        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(profile);
    }

    // Loads the default page or list view for this controller.

    public IActionResult Index()
    {
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(MyProfile));
    }

    [Authorize(Roles = "Admin")]
    // Loads user accounts for administrator review, verification, banning, or deletion.
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

            // Commits all pending EF Core changes to the SQL Server database.
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
        // Reads the currently logged-in Identity user ID from the authentication cookie.
        ViewBag.CurrentAdminUserId = _userManager.GetUserId(User);

        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(profiles);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    // Marks a selected user profile as verified after administrator review.
    public async Task<IActionResult> VerifyUser(int id)
    {
        return await SetVerificationStatusAsync(id, true);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    // Removes verified status from a selected user profile.
    public async Task<IActionResult> UnverifyUser(int id)
    {
        return await SetVerificationStatusAsync(id, false);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    // Handles the TempBanUser request using MVC action logic and returns the appropriate response.
    public async Task<IActionResult> TempBanUser(int id)
    {
        return await SetTemporaryBanStatusAsync(id, true);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    // Removes account lockout and allows the selected user to log in again.
    public async Task<IActionResult> UnbanUser(int id)
    {
        return await SetTemporaryBanStatusAsync(id, false);
    }

    // Handles the SetVerificationStatusAsync request using MVC action logic and returns the appropriate response.

    private async Task<IActionResult> SetVerificationStatusAsync(int id, bool verified)
    {
        // Retrieves a single matching database record asynchronously; returns null when not found.
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == id);
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (profile == null) return NotFound();

        var user = await _userManager.FindByIdAsync(profile.UserId);
        // Handles missing data safely before continuing with the requested operation.
        if (user == null)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Identity account was not found for the selected user.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(ManageUsers));
        }

        if (string.Equals(profile.RoleName, "Admin", StringComparison.OrdinalIgnoreCase) || await _userManager.IsInRoleAsync(user, "Admin"))
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Admin accounts are protected system accounts. Their verification status cannot be changed from User Management.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(ManageUsers));
        }

        var relatedProfiles = await _context.UserProfiles
            .Where(p => p.UserId == profile.UserId)
            .ToListAsync();

        foreach (var relatedProfile in relatedProfiles)
        {
            relatedProfile.IsVerified = verified;
        }

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = verified
            ? $"{profile.FullName} is now verified."
            : $"{profile.FullName} is now pending verification.";

        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(ManageUsers));
    }

    // Handles the SetTemporaryBanStatusAsync request using MVC action logic and returns the appropriate response.

    private async Task<IActionResult> SetTemporaryBanStatusAsync(int id, bool ban)
    {
        // Retrieves a single matching database record asynchronously; returns null when not found.
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == id);
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (profile == null) return NotFound();

        var user = await _userManager.FindByIdAsync(profile.UserId);
        // Handles missing data safely before continuing with the requested operation.
        if (user == null)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Identity account was not found for the selected user.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(ManageUsers));
        }

        // Reads the currently logged-in Identity user ID from the authentication cookie.
        var currentUserId = _userManager.GetUserId(User);
        if (user.Id == currentUserId)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "You cannot temporarily ban or unban your own currently logged-in admin account.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(ManageUsers));
        }

        if (string.Equals(profile.RoleName, "Admin", StringComparison.OrdinalIgnoreCase) || await _userManager.IsInRoleAsync(user, "Admin"))
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Admin accounts are protected and cannot be temporarily banned or unbanned from this page.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(ManageUsers));
        }

        user.LockoutEnabled = true;
        user.LockoutEnd = ban ? DateTimeOffset.UtcNow.AddDays(90) : null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Could not update the temporary ban status: " +
                                  string.Join(", ", result.Errors.Select(e => e.Description));
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(ManageUsers));
        }

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = ban
            ? $"{profile.FullName} has been temporarily banned from logging in."
            : $"{profile.FullName} has been unbanned and can log in again.";

        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(ManageUsers));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    // Handles the DeleteUser request using MVC action logic and returns the appropriate response.
    public async Task<IActionResult> DeleteUser(int id)
    {
        // Retrieves a single matching database record asynchronously; returns null when not found.
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == id);
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (profile == null) return NotFound();

        var user = await _userManager.FindByIdAsync(profile.UserId);
        // Handles missing data safely before continuing with the requested operation.
        if (user == null)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Identity account was not found. Please check this user manually.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(ManageUsers));
        }

        // Reads the currently logged-in Identity user ID from the authentication cookie.
        var currentUserId = _userManager.GetUserId(User);
        if (user.Id == currentUserId)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "You cannot delete your own admin account while logged in.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(ManageUsers));
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Admin accounts are protected and cannot be deleted from this page.";
            // Redirects the browser to another MVC action after the current operation is complete.
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
                    // Marks multiple related entities for deletion in one database save operation.
                    _context.UserRatings.RemoveRange(ratings);

                    var registrations = await _context.EventRegistrations
                        .Where(r => r.UserId == user.Id || organizedEventIds.Contains(r.VolunteerEventId))
                        .ToListAsync();
                    // Marks multiple related entities for deletion in one database save operation.
                    _context.EventRegistrations.RemoveRange(registrations);

                    var notifications = await _context.Notifications
                        .Where(n => profileIds.Contains(n.UserProfileId))
                        .ToListAsync();
                    // Marks multiple related entities for deletion in one database save operation.
                    _context.Notifications.RemoveRange(notifications);

                    var comments = await _context.PostComments
                        .Where(c => profileIds.Contains(c.UserProfileId) || postIds.Contains(c.CommunityPostId))
                        .ToListAsync();
                    // Marks multiple related entities for deletion in one database save operation.
                    _context.PostComments.RemoveRange(comments);

                    var posts = await _context.CommunityPosts
                        .Where(p => profileIds.Contains(p.UserProfileId))
                        .ToListAsync();
                    // Marks multiple related entities for deletion in one database save operation.
                    _context.CommunityPosts.RemoveRange(posts);

                    var helpRequests = await _context.HelpRequests
                        .Where(h => profileIds.Contains(h.UserProfileId))
                        .ToListAsync();
                    // Marks multiple related entities for deletion in one database save operation.
                    _context.HelpRequests.RemoveRange(helpRequests);

                    // Marks multiple related entities for deletion in one database save operation.
                    _context.VolunteerEvents.RemoveRange(organizedEvents);
                    // Marks multiple related entities for deletion in one database save operation.
                    _context.UserProfiles.RemoveRange(relatedProfiles);

                    // Commits all pending EF Core changes to the SQL Server database.
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

            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = $"{deletedFullName}'s account and all related data were deleted successfully.";
        }
        catch (Exception ex)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = ex.Message;
        }

        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(ManageUsers));
    }

    // Handles the GetPrimaryProfileForUserAsync request using MVC action logic and returns the appropriate response.

    private async Task<UserProfile?> GetPrimaryProfileForUserAsync(string userId)
    {
        return await _context.UserProfiles
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync();
    }

    // Handles the DeleteLocalFile request using MVC action logic and returns the appropriate response.

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
