// Handles account access: registration, login, logout, password change, and access denied.
// Technology map:
// - ASP.NET Core MVC actions return views or redirects.
// - ASP.NET Core Identity manages users, roles, sign-in sessions, and password security.
// - EF Core stores extra profile data in the UserProfiles table.
// Connected files: Register/Login/ChangePassword ViewModels, UserProfile model, Account views.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OCVMS.Data;
using OCVMS.Models;
using OCVMS.ViewModels;

namespace OCVMS.Controllers;

public class AccountController : Controller
{
    // ASP.NET Core Identity services used for user, role, and sign-in management.
    private readonly UserManager<IdentityUser> _userManager; // Identity service for user lookup, roles, and account operations.
    private readonly SignInManager<IdentityUser> _signInManager; // Identity service that creates and clears login sessions.
    private readonly RoleManager<IdentityRole> _roleManager; // Identity service for checking and creating roles.

    // Application database context used for storing profile-related data.
    private readonly ApplicationDbContext _context; // EF Core context connected to SQL Server tables.

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
    // Opens the registration form for new volunteers or organizers.
    public IActionResult Register()
    {
        // Displays the registration form for new public users.
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    // Creates a new Identity account, assigns a role, creates a profile, and signs the user in.
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Only volunteer and organizer accounts can be created through public registration.
        var allowedPublicRoles = new[] { "Volunteer", "Organizer" };

        if (!allowedPublicRoles.Contains(model.RoleName))
        {
            ModelState.AddModelError(nameof(model.RoleName), "Please select a valid role.");
        }

        // Organizer accounts must be linked with an organization name.
        if (model.RoleName == "Organizer" && string.IsNullOrWhiteSpace(model.OrganizationName))
        {
            ModelState.AddModelError(nameof(model.OrganizationName), "Organization name is required for organizer accounts.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Creates the Identity user account used for authentication.
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
            // Sends Identity validation errors back to the registration form.
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // Ensures the selected role exists before assigning it to the new user.
        if (!await _roleManager.RoleExistsAsync(model.RoleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(model.RoleName));
        }

        await _userManager.AddToRoleAsync(user, model.RoleName);

        var organizationName = model.RoleName == "Organizer"
            ? model.OrganizationName?.Trim()
            : null;

        // Stores application-specific profile details separately from Identity login data.
        _context.UserProfiles.Add(new UserProfile
        {
            UserId = user.Id,
            FullName = model.FullName.Trim(),
            PublicEmail = model.Email,
            RoleName = model.RoleName,
            OrganizationName = organizationName,
            IsVerified = false
        });

        await _context.SaveChangesAsync();

        // Signs in the user immediately after successful registration.
        await _signInManager.SignInAsync(user, isPersistent: false);

        TempData["Message"] = model.RoleName == "Organizer"
            ? "Organizer account created successfully. Your organization name was saved for secure ownership access."
            : "Account created successfully. Welcome to OCVMS!";

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    // Opens the login form.
    public IActionResult Login()
    {
        // Displays the login form.
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    // Validates login credentials and creates a secure authentication session.
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        // Prevents locked or banned accounts from creating a login session.
        if (user != null && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            ModelState.AddModelError(
                string.Empty,
                $"This account is temporarily banned from logging in until {user.LockoutEnd.Value.LocalDateTime:yyyy-MM-dd HH:mm}. Please contact the administrator.");

            return View(model);
        }

        // Uses a session-based login instead of keeping the user signed in permanently.
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
    // Opens the password change form for a signed-in user.
    public IActionResult ChangePassword()
    {
        // Displays the password change form for authenticated users.
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    // Changes the current user's password through ASP.NET Core Identity.
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

        // Updates the password through Identity after validating the current password.
        var result = await _userManager.ChangePasswordAsync(
            user,
            model.CurrentPassword,
            model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // Refreshes the authentication cookie so the user remains signed in after password change.
        await _signInManager.RefreshSignInAsync(user);

        TempData["Message"] = "Your password was changed successfully.";
        return RedirectToAction("MyProfile", "Profile");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    // Signs out the current user and clears the authentication session.
    public async Task<IActionResult> Logout()
    {
        // Clears the current authentication session.
        await _signInManager.SignOutAsync();

        TempData["Message"] = "You have logged out successfully.";
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    // Shows the access denied page when authorization fails.
    public IActionResult AccessDenied()
    {
        // Shows a friendly page for users who do not have permission to access a page.
        return View();
    }
}
