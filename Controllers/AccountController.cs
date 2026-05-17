// ================================================================
// VIVA COMMENTED VERSION - Controllers/AccountController.cs
// Purpose: Handles registration, login, logout, access denied page, and password change workflow.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OCVMS.Data;
using OCVMS.Models;
using OCVMS.ViewModels;

namespace OCVMS.Controllers;

public class AccountController : Controller
{
    // Dependencies injected through constructor for database, identity, hosting, or logging work.
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    // Registration action: creates a new user/account or registers the current user for an event depending on controller context.
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    // Registration action: creates a new user/account or registers the current user for an event depending on controller context.
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var allowedPublicRoles = new[] { "Volunteer", "Organizer" };
        if (!allowedPublicRoles.Contains(model.RoleName))
        {
            ModelState.AddModelError(nameof(model.RoleName), "Please select a valid role.");
        }

        if (model.RoleName == "Organizer" && string.IsNullOrWhiteSpace(model.OrganizationName))
        {
            ModelState.AddModelError(nameof(model.OrganizationName), "Organization name is required for organizer accounts.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        if (!await _roleManager.RoleExistsAsync(model.RoleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(model.RoleName));
        }

        await _userManager.AddToRoleAsync(user, model.RoleName);

        var organizationName = model.RoleName == "Organizer"
            ? model.OrganizationName?.Trim()
            : null;

        _context.UserProfiles.Add(new UserProfile
        {
            UserId = user.Id,
            FullName = model.FullName.Trim(),
            PublicEmail = model.Email,
            RoleName = model.RoleName,
            OrganizationName = organizationName,
            IsVerified = false
        });

        // Save all pending database changes.
        await _context.SaveChangesAsync();
        await _signInManager.SignInAsync(user, isPersistent: false);

        // TempData message is shown once after redirect.
            TempData["Message"] = model.RoleName == "Organizer"
            ? "Organizer account created successfully. Your organization name was saved for secure ownership access."
            : "Account created successfully. Welcome to OCVMS!";

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    // Login action: validates credentials and creates the authenticated user session.
    public IActionResult Login() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    // Login action: validates credentials and creates the authenticated user session.
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            ModelState.AddModelError(string.Empty, $"This account is temporarily banned from logging in until {user.LockoutEnd.Value.LocalDateTime:yyyy-MM-dd HH:mm}. Please contact the administrator.");
            return View(model);
        }

        // For better privacy on shared/lab computers, do not keep users signed in after the browser session ends.
        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account is temporarily banned from logging in. Please contact the administrator.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Email address or password is incorrect.");
        return View(model);
    }


    [HttpGet]
    [Authorize]
    // Password-change action: validates the old password and updates the account password securely using Identity.
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    // Password-change action: validates the old password and updates the account password securely using Identity.
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["Message"] = "Your password was changed successfully.";
        return RedirectToAction("MyProfile", "Profile");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    // Logout action: clears the current authenticated session.
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["Message"] = "You have logged out successfully.";
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    // Shows a friendly page when the logged-in user has no permission for a protected action.
    public IActionResult AccessDenied() => View();
}
