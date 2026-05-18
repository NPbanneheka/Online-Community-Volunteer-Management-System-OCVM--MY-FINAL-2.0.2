// Controllers/AccountController.cs
// This MVC controller file that receives browser requests and coordinates models, services, and views.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// ASP.NET Core authorization attributes such as [Authorize] and [AllowAnonymous].
using Microsoft.AspNetCore.Authorization;
// ASP.NET Core Identity services for users, roles, passwords, sign-in sessions, and lockout/ban behavior.
using Microsoft.AspNetCore.Identity;
// ASP.NET Core MVC base classes and action results used by controllers.
using Microsoft.AspNetCore.Mvc;
using OCVMS.Data;
using OCVMS.Models;
using OCVMS.ViewModels;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Controllers;

// Controller class: methods inside this class respond to user actions from the browser.
public class AccountController : Controller
{
    // _userManager works with ASP.NET Identity users, including lookup, creation, roles, and passwords.
    private readonly UserManager<IdentityUser> _userManager;
    // _signInManager creates, refreshes, and clears browser sign-in sessions.
    private readonly SignInManager<IdentityUser> _signInManager;
    // _roleManager checks and creates Identity roles such as Admin, Organizer, and Volunteer.
    private readonly RoleManager<IdentityRole> _roleManager;
    // _context connects this class to SQL Server through Entity Framework Core and ApplicationDbContext.
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
    // Handles user registration and stores both Identity login data and OCVMS profile data.
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    // Handles user registration and stores both Identity login data and OCVMS profile data.
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

        // Stops processing when form validation fails and returns the same view with validation messages.
        if (!ModelState.IsValid)
        {
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(model);
        }

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        // Creates a new Identity user and stores the password securely as a hash.
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(model);
        }

        // Checks whether the required Identity role already exists before creating or assigning it.
        if (!await _roleManager.RoleExistsAsync(model.RoleName))
        {
            // Creates a missing Identity role so role-based authorization can work correctly.
            await _roleManager.CreateAsync(new IdentityRole(model.RoleName));
        }

        // Assigns the selected Identity role to the user account.
        await _userManager.AddToRoleAsync(user, model.RoleName);

        var organizationName = model.RoleName == "Organizer"
            ? model.OrganizationName?.Trim()
            : null;

        // Adds a new entity to EF Core change tracking so it can be inserted into the database.
        // Creates an OCVMS profile record connected to an Identity user account.
        _context.UserProfiles.Add(new UserProfile
        {
            UserId = user.Id,
            FullName = model.FullName.Trim(),
            PublicEmail = model.Email,
            RoleName = model.RoleName,
            OrganizationName = organizationName,
            IsVerified = false
        });

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();
        await _signInManager.SignInAsync(user, isPersistent: false);

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = model.RoleName == "Organizer"
            ? "Organizer account created successfully. Your organization name was saved for secure ownership access."
            : "Account created successfully. Welcome to OCVMS!";

        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    // Authenticates the user and creates a secure sign-in session through ASP.NET Core Identity.
    public IActionResult Login() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    // Authenticates the user and creates a secure sign-in session through ASP.NET Core Identity.
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Stops processing when form validation fails and returns the same view with validation messages.
        if (!ModelState.IsValid)
        {
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(model);
        }

        // Looks up an Identity user account by email address.
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            ModelState.AddModelError(string.Empty, $"This account is temporarily banned from logging in until {user.LockoutEnd.Value.LocalDateTime:yyyy-MM-dd HH:mm}. Please contact the administrator.");
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(model);
        }

        // For better privacy on shared/lab computers, do not keep users signed in after the browser session ends.
        // Validates credentials and creates the authenticated session when the password is correct.
        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account is temporarily banned from logging in. Please contact the administrator.");
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Email address or password is incorrect.");
        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(model);
    }

    [HttpGet]
    [Authorize]
    // Allows an authenticated user to update their password using Identity password validation.
    public IActionResult ChangePassword()
    {
        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    // Allows an authenticated user to update their password using Identity password validation.
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        // Stops processing when form validation fails and returns the same view with validation messages.
        if (!ModelState.IsValid)
        {
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        // Handles missing data safely before continuing with the requested operation.
        if (user == null)
        {
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(Login));
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(model);
        }

        // Refreshes the sign-in cookie after account details such as password are changed.
        await _signInManager.RefreshSignInAsync(user);
        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Your password was changed successfully.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction("MyProfile", "Profile");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    // Clears the current authentication session and redirects the user to the home page.
    public async Task<IActionResult> Logout()
    {
        // Ends the current authenticated session.
        await _signInManager.SignOutAsync();
        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "You have logged out successfully.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    // Displays a friendly page when an authenticated user does not have permission.
    public IActionResult AccessDenied() => View();
}
