// Controllers/CommunityController.cs
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
public class CommunityController : Controller
{
    // _context connects this class to SQL Server through Entity Framework Core and ApplicationDbContext.
    private readonly ApplicationDbContext _context;
    // _userManager works with ASP.NET Identity users, including lookup, creation, roles, and passwords.
    private readonly UserManager<IdentityUser> _userManager;

    public CommunityController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Loads the community feed with filtering, sorting, related comments, and permission data for the view.

    public async Task<IActionResult> Feed(string? filter, string? sortOrder)
    {
        filter = string.IsNullOrWhiteSpace(filter) ? "All" : filter;
        sortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "Newest" : sortOrder;

        var postsQuery = _context.CommunityPosts
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(p => p.User)
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(p => p.PostComments)
                // ThenInclude loads deeper related data from a previously included navigation property.
                .ThenInclude(c => c.User)
            .AsQueryable();

        if (filter == "Help")
        {
            postsQuery = postsQuery.Where(p => p.PostType == "Help");
        }
        else if (filter == "Share")
        {
            postsQuery = postsQuery.Where(p => p.PostType != "Help");
        }

        postsQuery = sortOrder == "Oldest"
            ? postsQuery.OrderBy(p => p.CreatedAt)
            : postsQuery.OrderByDescending(p => p.CreatedAt);

        // Executes the EF Core query asynchronously and stores the result as a list.
        var posts = await postsQuery.ToListAsync();
        // Executes the EF Core query asynchronously and stores the result as a list.
        var allPosts = await _context.CommunityPosts.ToListAsync();

        var currentProfile = await GetCurrentProfileAsync();
        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
        var isAdmin = User.IsInRole("Admin");

        ViewBag.CurrentProfile = currentProfile;
        ViewBag.CurrentProfileId = currentProfile?.Id;
        ViewBag.CurrentOrganizationName = currentProfile?.OrganizationName;
        ViewBag.IsAdmin = isAdmin;
        ViewBag.PostCount = currentProfile != null ? allPosts.Count(p => p.UserProfileId == currentProfile.Id) : 0;
        ViewBag.TotalPostCount = allPosts.Count;
        ViewBag.HelpPostCount = allPosts.Count(p => p.PostType == "Help");
        ViewBag.MyHelpPostCount = currentProfile != null ? allPosts.Count(p => p.UserProfileId == currentProfile.Id && p.PostType == "Help") : 0;
        ViewBag.Filter = filter;
        ViewBag.SortOrder = sortOrder;
        ViewBag.ManageablePostIds = posts
            .Where(p => CanManagePost(p, currentProfile, isAdmin))
            .Select(p => p.Id)
            .ToList();
        ViewBag.ManageableCommentIds = posts
            .SelectMany(p => p.PostComments.Select(c => new { Post = p, Comment = c }))
            .Where(x => isAdmin || (currentProfile != null && (x.Comment.UserProfileId == currentProfile.Id || CanManagePost(x.Post, currentProfile, false))))
            .Select(x => x.Comment.Id)
            .ToList();

        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(posts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Creates a community post and sends notifications when the post is marked as a help request.
    public async Task<IActionResult> CreatePost(string title, string content, string postType)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Error"] = "Title and content cannot be empty.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(Feed));
        }

        var userProfile = await GetCurrentProfileAsync();
        // Handles missing data safely before continuing with the requested operation.
        if (userProfile == null)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Error"] = "Please complete your user profile before posting.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction("Edit", "Profile");
        }

        postType = postType == "Help" ? "Help" : "Share";

        var post = new CommunityPost
        {
            Title = title.Trim(),
            Content = content.Trim(),
            PostType = postType,
            UserProfileId = userProfile.Id
        };

        // Adds a new entity to EF Core change tracking so it can be inserted into the database.
        _context.CommunityPosts.Add(post);

        if (postType == "Help")
        {
            var notifyProfiles = await _context.UserProfiles
                .Where(p => p.RoleName == "Admin" || p.RoleName == "Organizer")
                .ToListAsync();

            foreach (var profile in notifyProfiles)
            {
                // Adds a new entity to EF Core change tracking so it can be inserted into the database.
                // Creates a notification record connected to a user profile.
                _context.Notifications.Add(new Notification
                {
                    UserProfileId = profile.Id,
                    Message = $"Community help post created: {post.Title}."
                });
            }
        }

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Post published successfully!";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(Feed));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Adds a comment to a community post and notifies the original post owner when required.
    public async Task<IActionResult> AddComment(int postId, string commentContent)
    {
        if (string.IsNullOrWhiteSpace(commentContent))
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Error"] = "Comment cannot be empty.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(Feed));
        }

        // Retrieves a single matching database record asynchronously; returns null when not found.
        var post = await _context.CommunityPosts.FirstOrDefaultAsync(p => p.Id == postId);
        // Handles missing data safely before continuing with the requested operation.
        if (post == null)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Error"] = "The selected post was not found.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction(nameof(Feed));
        }

        var userProfile = await GetCurrentProfileAsync();
        // Handles missing data safely before continuing with the requested operation.
        if (userProfile == null)
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Error"] = "Please complete your user profile before commenting.";
            // Redirects the browser to another MVC action after the current operation is complete.
            return RedirectToAction("Edit", "Profile");
        }

        // Adds a new entity to EF Core change tracking so it can be inserted into the database.
        _context.PostComments.Add(new PostComment
        {
            Content = commentContent.Trim(),
            CommunityPostId = postId,
            UserProfileId = userProfile.Id
        });

        if (post.UserProfileId != userProfile.Id)
        {
            // Adds a new entity to EF Core change tracking so it can be inserted into the database.
            // Creates a notification record connected to a user profile.
            _context.Notifications.Add(new Notification
            {
                UserProfileId = post.UserProfileId,
                Message = $"{userProfile.FullName} commented on your community post: {post.Title}."
            });
        }

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();

        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(Feed));
    }

    [HttpGet]
    // Loads or updates a community post after checking ownership or administrator permission.
    public async Task<IActionResult> EditPost(int id)
    {
        var post = await _context.CommunityPosts
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (post == null) return NotFound();

        // Enforces ownership or role-based permission before allowing data changes.
        if (!await CanManagePostAsync(post))
        {
            // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
            return Forbid();
        }

        // Sends data to a Razor view so the page can be rendered in the browser.
        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Loads or updates a community post after checking ownership or administrator permission.
    public async Task<IActionResult> EditPost(int id, string title, string content, string postType)
    {
        var post = await _context.CommunityPosts
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (post == null) return NotFound();

        // Enforces ownership or role-based permission before allowing data changes.
        if (!await CanManagePostAsync(post))
        {
            // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            // TempData stores a one-time message that is displayed after redirecting to another page.
            TempData["Error"] = "Title and content cannot be empty.";
            // Sends data to a Razor view so the page can be rendered in the browser.
            return View(post);
        }

        post.Title = title.Trim();
        post.Content = content.Trim();
        post.PostType = postType == "Help" ? "Help" : "Share";

        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Post updated successfully.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(Feed));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Deletes a community post after verifying that the current user is allowed to manage it.
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _context.CommunityPosts
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(p => p.User)
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(p => p.PostComments)
            .FirstOrDefaultAsync(p => p.Id == id);

        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (post == null) return NotFound();

        // Enforces ownership or role-based permission before allowing data changes.
        if (!await CanManagePostAsync(post))
        {
            // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
            return Forbid();
        }

        if (post.PostComments.Any())
        {
            // Marks multiple related entities for deletion in one database save operation.
            _context.PostComments.RemoveRange(post.PostComments);
        }

        // Marks the selected entity for deletion from the database.
        _context.CommunityPosts.Remove(post);
        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Post deleted successfully.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(Feed));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Deletes a comment when the current user is the comment owner, post owner, or administrator.
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await _context.PostComments
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(c => c.User)
            // Include loads related table data so the view can access connected records without extra queries.
            .Include(c => c.Post)
                // ThenInclude loads deeper related data from a previously included navigation property.
                .ThenInclude(p => p!.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        // Handles missing data safely before continuing with the requested operation.
        // Returns HTTP 404 when the requested record does not exist.
        if (comment == null) return NotFound();

        // Enforces ownership or role-based permission before allowing data changes.
        if (!await CanManageCommentAsync(comment))
        {
            // Returns HTTP 403 when the user is authenticated but not allowed to perform this action.
            return Forbid();
        }

        // Marks the selected entity for deletion from the database.
        _context.PostComments.Remove(comment);
        // Commits all pending EF Core changes to the SQL Server database.
        await _context.SaveChangesAsync();

        // TempData stores a one-time message that is displayed after redirecting to another page.
        TempData["Message"] = "Comment deleted successfully.";
        // Redirects the browser to another MVC action after the current operation is complete.
        return RedirectToAction(nameof(Feed));
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

    // Handles the CanManagePostAsync request using MVC action logic and returns the appropriate response.

    private async Task<bool> CanManagePostAsync(CommunityPost post)
    {
        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
        if (User.IsInRole("Admin")) return true;

        var profile = await GetCurrentProfileAsync();
        // Handles missing data safely before continuing with the requested operation.
        if (profile == null) return false;

        // Handles missing data safely before continuing with the requested operation.
        if (post.User == null)
        {
            post.User = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == post.UserProfileId);
        }

        return CanManagePost(post, profile, false);
    }

    // Handles the CanManageCommentAsync request using MVC action logic and returns the appropriate response.

    private async Task<bool> CanManageCommentAsync(PostComment comment)
    {
        // Checks the logged-in user's role using ASP.NET Core Identity role membership.
        if (User.IsInRole("Admin")) return true;

        var profile = await GetCurrentProfileAsync();
        // Handles missing data safely before continuing with the requested operation.
        if (profile == null) return false;

        if (comment.UserProfileId == profile.Id) return true;

        if (comment.Post != null)
        {
            // Handles missing data safely before continuing with the requested operation.
            if (comment.Post.User == null)
            {
                comment.Post.User = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == comment.Post.UserProfileId);
            }

            return CanManagePost(comment.Post, profile, false);
        }

        return false;
    }

    // Handles the CanManagePost request using MVC action logic and returns the appropriate response.

    private static bool CanManagePost(CommunityPost post, UserProfile? currentProfile, bool isAdmin)
    {
        if (isAdmin) return true;
        // Handles missing data safely before continuing with the requested operation.
        if (currentProfile == null) return false;

        // Final ownership rule: users can edit/delete only their own posts. Admin can manage all.
        return post.UserProfileId == currentProfile.Id;
    }

    // Handles the CanManageComment request using MVC action logic and returns the appropriate response.

    private static bool CanManageComment(PostComment comment, UserProfile? currentProfile, bool isAdmin)
    {
        if (isAdmin) return true;
        // Handles missing data safely before continuing with the requested operation.
        if (currentProfile == null) return false;

        // Comment owner can delete their own comment. The post owner can also remove comments from their own post.
        if (comment.UserProfileId == currentProfile.Id) return true;
        return comment.Post != null && CanManagePost(comment.Post, currentProfile, false);
    }
}
