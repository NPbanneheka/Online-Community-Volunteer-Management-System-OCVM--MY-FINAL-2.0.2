<<<<<<< HEAD
// Controllers/AccountController.cs
// This MVC controller file that receives browser requests and coordinates models, services, and views.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// ASP.NET Core authorization attributes such as [Authorize] and [AllowAnonymous].
=======
// Handles account access: registration, login, logout, password change, and access denied.
// Technology map:
// - ASP.NET Core MVC actions return views or redirects.
// - ASP.NET Core Identity manages users, roles, sign-in sessions, and password security.
// - EF Core stores extra profile data in the UserProfiles table.
// Connected files: Register/Login/ChangePassword ViewModels, UserProfile model, Account views.

>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
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
<<<<<<< HEAD
    // _userManager works with ASP.NET Identity users, including lookup, creation, roles, and passwords.
    private readonly UserManager<IdentityUser> _userManager;
    // _signInManager creates, refreshes, and clears browser sign-in sessions.
    private readonly SignInManager<IdentityUser> _signInManager;
    // _roleManager checks and creates Identity roles such as Admin, Organizer, and Volunteer.
    private readonly RoleManager<IdentityRole> _roleManager;
    // _context connects this class to SQL Server through Entity Framework Core and ApplicationDbContext.
    private readonly ApplicationDbContext _context;
=======
    // ASP.NET Core Identity services used for user, role, and sign-in management.
    private readonly UserManager<IdentityUser> _userManager; // Identity service for user lookup, roles, and account operations.
    private readonly SignInManager<IdentityUser> _signInManager; // Identity service that creates and clears login sessions.
    private readonly RoleManager<IdentityRole> _roleManager; // Identity service for checking and creating roles.

    // Application database context used for storing profile-related data.
    private readonly ApplicationDbContext _context; // EF Core context connected to SQL Server tables.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675

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
<<<<<<< HEAD
    // Handles user registration and stores both Identity login data and OCVMS profile data.
    public IActionResult Register() => View(new RegisterViewModel());
=======
    // Opens the registration form for new volunteers or organizers.
    public IActionResult Register()
    {
        // Displays the registration form for new public users.
        return View(new RegisterViewModel());
    }
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
<<<<<<< HEAD
    // Handles user registration and stores both Identity login data and OCVMS profile data.
=======
    // Creates a new Identity account, assigns a role, creates a profile, and signs the user in.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
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

        // Stops processing when form validation fails and returns the same view with validation messages.
        if (!ModelState.IsValid)
        {
            // Sends data to a Razor view so the page can be rendered in the browser.
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

        // Creates a new Identity user and stores the password securely as a hash.
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            // Sends Identity validation errors back to the registration form.
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
<<<<<<< HEAD
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(model);
        }

        // Checks whether the required Identity role already exists before creating or assigning it.
=======

            return View(model);
        }

        // Ensures the selected role exists before assigning it to the new user.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
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

<<<<<<< HEAD
        // Adds a new entity to EF Core change tracking so it can be inserted into the database.
        // Creates an OCVMS profile record connected to an Identity user account.
=======
        // Stores application-specific profile details separately from Identity login data.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
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

        // Signs in the user immediately after successful registration.
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
<<<<<<< HEAD
    // Authenticates the user and creates a secure sign-in session through ASP.NET Core Identity.
    public IActionResult Login() => View();
=======
    // Opens the login form.
    public IActionResult Login()
    {
        // Displays the login form.
        return View();
    }
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
<<<<<<< HEAD
    // Authenticates the user and creates a secure sign-in session through ASP.NET Core Identity.
=======
    // Validates login credentials and creates a secure authentication session.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
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

        // Prevents locked or banned accounts from creating a login session.
        if (user != null && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
<<<<<<< HEAD
            ModelState.AddModelError(string.Empty, $"This account is temporarily banned from logging in until {user.LockoutEnd.Value.LocalDateTime:yyyy-MM-dd HH:mm}. Please contact the administrator.");
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(model);
        }

        // For better privacy on shared/lab computers, do not keep users signed in after the browser session ends.
        // Validates credentials and creates the authenticated session when the password is correct.
=======
            ModelState.AddModelError(
                string.Empty,
                $"This account is temporarily banned from logging in until {user.LockoutEnd.Value.LocalDateTime:yyyy-MM-dd HH:mm}. Please contact the administrator.");

            return View(model);
        }

        // Uses a session-based login instead of keeping the user signed in permanently.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
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
<<<<<<< HEAD
    // Allows an authenticated user to update their password using Identity password validation.
    public IActionResult ChangePassword()
    {
        // Sends data to a Razor view so the page can be rendered in the browser.
=======
    // Opens the password change form for a signed-in user.
    public IActionResult ChangePassword()
    {
        // Displays the password change form for authenticated users.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
<<<<<<< HEAD
    // Allows an authenticated user to update their password using Identity password validation.
=======
    // Changes the current user's password through ASP.NET Core Identity.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        // Stops processing when form validation fails and returns the same view with validation messages.
        if (!ModelState.IsValid)
        {
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
<<<<<<< HEAD
        // Handles missing data safely before continuing with the requested operation.
=======

>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
        if (user == null)
        {
            // Redirects the browser to another MVC action after the current operation is complete.
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
<<<<<<< HEAD
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(model);
        }

        // Refreshes the sign-in cookie after account details such as password are changed.
        await _signInManager.RefreshSignInAsync(user);
        // TempData stores a one-time message that is displayed after redirecting to another page.
=======

            return View(model);
        }

        // Refreshes the authentication cookie so the user remains signed in after password change.
        await _signInManager.RefreshSignInAsync(user);

>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
        TempData["Message"] = "Your password was changed successfully.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction("MyProfile", "Profile");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
<<<<<<< HEAD
    // Clears the current authentication session and redirects the user to the home page.
    public async Task<IActionResult> Logout()
    {
        // Ends the current authenticated session.
        await _signInManager.SignOutAsync();
        // TempData stores a one-time message that is displayed after redirecting to another page.
=======
    // Signs out the current user and clears the authentication session.
    public async Task<IActionResult> Logout()
    {
        // Clears the current authentication session.
        await _signInManager.SignOutAsync();

>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
        TempData["Message"] = "You have logged out successfully.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
<<<<<<< HEAD
    // Displays a friendly page when an authenticated user does not have permission.
    public IActionResult AccessDenied() => View();
=======
    // Shows the access denied page when authorization fails.
    public IActionResult AccessDenied()
    {
        // Shows a friendly page for users who do not have permission to access a page.
        return View();
    }
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
}
