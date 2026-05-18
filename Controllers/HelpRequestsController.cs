// Controllers/HelpRequestsController.cs
// This MVC controller file that receives browser requests and coordinates models, services, and views.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// ASP.NET Core authorization attributes such as [Authorize] and [AllowAnonymous].
using Microsoft.AspNetCore.Authorization;
// ASP.NET Core Identity services for users, roles, passwords, sign-in sessions, and lockout/ban behavior.
using Microsoft.AspNetCore.Identity;
// ASP.NET Core MVC base classes and action results used by controllers.
using Microsoft.AspNetCore.Mvc;
// Entity Framework Core features such as Include(), Where(), ToListAsync(), and database queries.
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Controllers;

[Authorize]
// Controller class: methods inside this class respond to user actions from the browser.
public class HelpRequestsController : Controller
{
    // _context connects this class to SQL Server through Entity Framework Core and ApplicationDbContext.
    private readonly ApplicationDbContext _context;
    // _userManager works with ASP.NET Identity users, including lookup, creation, roles, and passwords.
    private readonly UserManager<IdentityUser> _userManager;

    public HelpRequestsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Loads the default page or list view for this controller.

    public async Task<IActionResult> Index()
    {
        var profile = await GetCurrentProfileAsync();
        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
        var isAdmin = User.IsInRole("Admin");

        var allRequests = await _context.HelpRequests
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(h => h.SubmittedBy)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

        var requests = allRequests
            .Where(h => CanViewHelpRequest(h, profile, isAdmin))
            .ToList();

        ViewBag.CurrentProfileId = profile?.Id;
        ViewBag.CurrentOrganizationName = profile?.OrganizationName;
        ViewBag.IsAdmin = isAdmin;
        ViewBag.ManageableHelpRequestIds = requests
            .Where(h => CanManageHelpRequest(h, profile, isAdmin))
            .Select(h => h.Id)
            .ToList();
        ViewBag.StatusManageableHelpRequestIds = requests
            .Where(h => CanUpdateHelpRequestStatus(h, profile, isAdmin))
            .Select(h => h.Id)
            .ToList();

        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(requests);
    }

    // Displays or processes the form used to create a new record.

    public IActionResult Create()
    {
        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(new HelpRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Displays or processes the form used to create a new record.
    public async Task<IActionResult> Create(HelpRequest helpRequest)
    {
        var profile = await GetCurrentProfileAsync();

        // Handles missing data safely before continuing with the requested operation.
        if (profile == null)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Please complete your profile before submitting a support request.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction("Edit", "Profile");
        }

        helpRequest.UserProfileId = profile.Id;
        helpRequest.Status = "Pending";

        ModelState.Remove(nameof(HelpRequest.SubmittedBy));
        ModelState.Remove(nameof(HelpRequest.UserProfileId));
        ModelState.Remove(nameof(HelpRequest.Status));

        // Stops processing when form validation fails and returns the same view with validation messages.
        if (!ModelState.IsValid)
        {
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(helpRequest);
        }

        // Adds a new entity to EF Core change tracking so it can be inserted into the database.
        _context.HelpRequests.Add(helpRequest);

        var notifyProfiles = await _context.UserProfiles
            .Where(p => p.RoleName == "Admin")
            .ToListAsync();

        foreach (var notifyProfile in notifyProfiles)
        {
            // Adds a new entity to EF Core change tracking so it can be inserted into the database.
            // Creates a notification record connected to a user profile.
            _context.Notifications.Add(new Notification
            {
                UserProfileId = notifyProfile.Id,
                Message = $"New support request submitted: {helpRequest.Title}."
            });
        }

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Support request submitted successfully.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(Index));
    }

    // Loads a single record with related data for a detail page.

    public async Task<IActionResult> Details(int? id)
    {
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (id == null) return NotFound();

        var helpRequest = await _context.HelpRequests
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(h => h.SubmittedBy)
            .FirstOrDefaultAsync(h => h.Id == id);

        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (helpRequest == null) return NotFound();

        // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
        if (!await CanViewHelpRequestAsync(helpRequest)) return Forbid();

        ViewBag.CanManageHelpRequest = await CanManageHelpRequestAsync(helpRequest);
        ViewBag.CanUpdateHelpRequestStatus = await CanUpdateHelpRequestStatusAsync(helpRequest);
        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
        ViewBag.IsAdmin = User.IsInRole("Admin");

        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(helpRequest);
    }

    [HttpGet]
    // Displays or processes the form used to update an existing record.
    public async Task<IActionResult> Edit(int? id)
    {
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (id == null) return NotFound();

        var helpRequest = await _context.HelpRequests
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(h => h.SubmittedBy)
            .FirstOrDefaultAsync(h => h.Id == id);
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (helpRequest == null) return NotFound();

        // Enforces ownership or role-based permission before allowing data changes.
        if (!await CanManageHelpRequestAsync(helpRequest))
        {
            // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
            return Forbid();
        }

        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(helpRequest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Displays or processes the form used to update an existing record.
    public async Task<IActionResult> Edit(int id, HelpRequest helpRequest)
    {
        // Returns HTTP 404 when the requested record does not exist.
        if (id != helpRequest.Id) return NotFound();

        var existingRequest = await _context.HelpRequests
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(h => h.SubmittedBy)
            .FirstOrDefaultAsync(h => h.Id == id);
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (existingRequest == null) return NotFound();

        // Enforces ownership or role-based permission before allowing data changes.
        if (!await CanManageHelpRequestAsync(existingRequest))
        {
            // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
            return Forbid();
        }

        ModelState.Remove(nameof(HelpRequest.SubmittedBy));
        ModelState.Remove(nameof(HelpRequest.UserProfileId));
        ModelState.Remove(nameof(HelpRequest.Status));

        // Stops processing when form validation fails and returns the same view with validation messages.
        if (!ModelState.IsValid)
        {
            helpRequest.UserProfileId = existingRequest.UserProfileId;
            helpRequest.Status = existingRequest.Status;
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(helpRequest);
        }

        existingRequest.Title = helpRequest.Title;
        existingRequest.Description = helpRequest.Description;

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Support request updated successfully.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(Details), new { id = existingRequest.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Deletes an existing record after validation and authorization checks.
    public async Task<IActionResult> Delete(int id)
    {
        var helpRequest = await _context.HelpRequests
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(h => h.SubmittedBy)
            .FirstOrDefaultAsync(h => h.Id == id);
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (helpRequest == null) return NotFound();

        // Enforces ownership or role-based permission before allowing data changes.
        if (!await CanManageHelpRequestAsync(helpRequest))
        {
            // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
            return Forbid();
        }

        // Marks the selected entity for deletion from the database.
        _context.HelpRequests.Remove(helpRequest);
        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Support request deleted successfully.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    // Handles the UpdateStatus request using MVC action logic and returns the appropriate response.
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var allowedStatuses = new[] { "Pending", "In Progress", "Resolved", "Closed" };
        if (!allowedStatuses.Contains(status))
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Message"] = "Invalid status selected.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(Index));
        }

        var helpRequest = await _context.HelpRequests
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(h => h.SubmittedBy)
            .FirstOrDefaultAsync(h => h.Id == id);
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (helpRequest == null) return NotFound();

        if (!await CanUpdateHelpRequestStatusAsync(helpRequest))
        {
            // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
            return Forbid();
        }

        helpRequest.Status = status;

        // Adds a new entity to EF Core change tracking so it can be inserted into the database.
        // Creates a notification record connected to a user profile.
        _context.Notifications.Add(new Notification
        {
            UserProfileId = helpRequest.UserProfileId,
            Message = $"Your support request '{helpRequest.Title}' status changed to {status}."
        });

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Support request status updated.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(Details), new { id });
    }

    // Handles the GetCurrentProfileAsync request using MVC action logic and returns the appropriate response.

    private async Task<UserProfile?> GetCurrentProfileAsync()
    {
        // Reads the currently logged-in Identity user ID from the authentication cookie.
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return null;
        return await GetPrimaryProfileForUserAsync(userId);
    }

    // Handles the GetPrimaryProfileForUserAsync request using MVC action logic and returns the appropriate response.

    private async Task<UserProfile?> GetPrimaryProfileForUserAsync(string userId)
    {
        return await _context.UserProfiles
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsVerified)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    // Handles the CanViewHelpRequestAsync request using MVC action logic and returns the appropriate response.

    private async Task<bool> CanViewHelpRequestAsync(HelpRequest helpRequest)
    {
        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
        if (User.IsInRole("Admin")) return true;
        var profile = await GetCurrentProfileAsync();
        return CanViewHelpRequest(helpRequest, profile, false);
    }

    // Handles the CanManageHelpRequestAsync request using MVC action logic and returns the appropriate response.

    private async Task<bool> CanManageHelpRequestAsync(HelpRequest helpRequest)
    {
        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
        if (User.IsInRole("Admin")) return true;
        var profile = await GetCurrentProfileAsync();
        return CanManageHelpRequest(helpRequest, profile, false);
    }

    // Handles the CanUpdateHelpRequestStatusAsync request using MVC action logic and returns the appropriate response.

    private async Task<bool> CanUpdateHelpRequestStatusAsync(HelpRequest helpRequest)
    {
        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
        if (User.IsInRole("Admin")) return true;
        var profile = await GetCurrentProfileAsync();
        return CanUpdateHelpRequestStatus(helpRequest, profile, false);
    }

    // Handles the CanViewHelpRequest request using MVC action logic and returns the appropriate response.

    private static bool CanViewHelpRequest(HelpRequest helpRequest, UserProfile? currentProfile, bool isAdmin)
    {
        if (isAdmin) return true;
        // Handles missing data safely before continuing with the requested operation.
        if (currentProfile == null) return false;

        // Support Requests are for system/app issues.
        // Users can view only their own support requests; Admin can view all.
        return helpRequest.UserProfileId == currentProfile.Id;
    }

    // Handles the CanManageHelpRequest request using MVC action logic and returns the appropriate response.

    private static bool CanManageHelpRequest(HelpRequest helpRequest, UserProfile? currentProfile, bool isAdmin)
    {
        if (isAdmin) return true;
        // Handles missing data safely before continuing with the requested operation.
        if (currentProfile == null) return false;

        // Only the request owner or Admin can edit/delete a support request.
        return helpRequest.UserProfileId == currentProfile.Id;
    }

    // Handles the CanUpdateHelpRequestStatus request using MVC action logic and returns the appropriate response.

    private static bool CanUpdateHelpRequestStatus(HelpRequest helpRequest, UserProfile? currentProfile, bool isAdmin)
    {
        // Support request status is handled by Admin/system support side only.
        return isAdmin;
    }
}
