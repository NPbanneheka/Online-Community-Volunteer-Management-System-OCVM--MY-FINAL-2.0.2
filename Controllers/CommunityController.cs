// Handles the community feed, posts, comments, and post-related notifications.
// Technology map:
// - ASP.NET Core MVC handles form posts and page navigation.
// - EF Core LINQ queries load posts, comments, authors, and notifications.
// - Authorization ensures only signed-in users can access community actions.
// Connected files: CommunityPost, PostComment, Notification, UserProfile models and Community views.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;

namespace OCVMS.Controllers;

[Authorize]
public class CommunityController : Controller
{
    // Database context for community posts, comments, profiles, and notifications.
    private readonly ApplicationDbContext _context; // EF Core context connected to SQL Server tables.

    // Identity service used to identify the currently logged-in user.
    private readonly UserManager<IdentityUser> _userManager; // Identity service for user lookup, roles, and account operations.

    public CommunityController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Loads the community feed with filtering, sorting, statistics, and permission lists.
    public async Task<IActionResult> Feed(string? filter, string? sortOrder)
    {
        // Applies default feed options when no filter or sort value is provided.
        filter = string.IsNullOrWhiteSpace(filter) ? "All" : filter;
        sortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "Newest" : sortOrder;

        // Builds the community feed query with related author and comment details.
        var postsQuery = _context.CommunityPosts
            .Include(p => p.User)
            .Include(p => p.PostComments)
                .ThenInclude(c => c.User)
            .AsQueryable();

        // Filters the feed by post type.
        if (filter == "Help")
        {
            postsQuery = postsQuery.Where(p => p.PostType == "Help");
        }
        else if (filter == "Share")
        {
            postsQuery = postsQuery.Where(p => p.PostType != "Help");
        }

        // Orders posts based on the selected sort option.
        postsQuery = sortOrder == "Oldest"
            ? postsQuery.OrderBy(p => p.CreatedAt)
            : postsQuery.OrderByDescending(p => p.CreatedAt);

        var posts = await postsQuery.ToListAsync();
        var allPosts = await _context.CommunityPosts.ToListAsync();

        var currentProfile = await GetCurrentProfileAsync();
        var isAdmin = User.IsInRole("Admin");

        // Supplies feed statistics and permission-related data to the Razor view.
        ViewBag.CurrentProfile = currentProfile;
        ViewBag.CurrentProfileId = currentProfile?.Id;
        ViewBag.CurrentOrganizationName = currentProfile?.OrganizationName;
        ViewBag.IsAdmin = isAdmin;
        ViewBag.PostCount = currentProfile != null
            ? allPosts.Count(p => p.UserProfileId == currentProfile.Id)
            : 0;
        ViewBag.TotalPostCount = allPosts.Count;
        ViewBag.HelpPostCount = allPosts.Count(p => p.PostType == "Help");
        ViewBag.MyHelpPostCount = currentProfile != null
            ? allPosts.Count(p => p.UserProfileId == currentProfile.Id && p.PostType == "Help")
            : 0;
        ViewBag.Filter = filter;
        ViewBag.SortOrder = sortOrder;

        // Identifies which posts the current user is allowed to edit or delete.
        ViewBag.ManageablePostIds = posts
            .Where(p => CanManagePost(p, currentProfile, isAdmin))
            .Select(p => p.Id)
            .ToList();

        // Identifies which comments the current user is allowed to delete.
        ViewBag.ManageableCommentIds = posts
            .SelectMany(p => p.PostComments.Select(c => new { Post = p, Comment = c }))
            .Where(x =>
                isAdmin ||
                (currentProfile != null &&
                    (x.Comment.UserProfileId == currentProfile.Id ||
                     CanManagePost(x.Post, currentProfile, false))))
            .Select(x => x.Comment.Id)
            .ToList();

        return View(posts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Creates a community post and notifies relevant users when it is a help post.
    public async Task<IActionResult> CreatePost(string title, string content, string postType)
    {
        // Validates required post content before creating a database record.
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
        TempData["Error"] = "Title and content cannot be empty.";
            return RedirectToAction(nameof(Feed));
        }

        var userProfile = await GetCurrentProfileAsync();

        if (userProfile == null)
        {
        TempData["Error"] = "Please complete your user profile before posting.";
            return RedirectToAction("Edit", "Profile");
        }

        // Keeps post type limited to the supported community categories.
        postType = postType == "Help" ? "Help" : "Share";

        var post = new CommunityPost
        {
            Title = title.Trim(),
            Content = content.Trim(),
            PostType = postType,
            UserProfileId = userProfile.Id
        };

        _context.CommunityPosts.Add(post);

        // Help posts notify admins and organizers because they may require follow-up action.
        if (postType == "Help")
        {
            var notifyProfiles = await _context.UserProfiles
                .Where(p => p.RoleName == "Admin" || p.RoleName == "Organizer")
                .ToListAsync();

            foreach (var profile in notifyProfiles)
            {
                _context.Notifications.Add(new Notification
                {
                    UserProfileId = profile.Id,
                    Message = $"Community help post created: {post.Title}."
                });
            }
        }

        await _context.SaveChangesAsync();

        TempData["Message"] = "Post published successfully!";
        return RedirectToAction(nameof(Feed));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Adds a comment to a community post and notifies the post owner.
    public async Task<IActionResult> AddComment(int postId, string commentContent)
    {
        // Prevents empty comments from being saved.
        if (string.IsNullOrWhiteSpace(commentContent))
        {
        TempData["Error"] = "Comment cannot be empty.";
            return RedirectToAction(nameof(Feed));
        }

        var post = await _context.CommunityPosts.FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
        {
        TempData["Error"] = "The selected post was not found.";
            return RedirectToAction(nameof(Feed));
        }

        var userProfile = await GetCurrentProfileAsync();

        if (userProfile == null)
        {
        TempData["Error"] = "Please complete your user profile before commenting.";
            return RedirectToAction("Edit", "Profile");
        }

        _context.PostComments.Add(new PostComment
        {
            Content = commentContent.Trim(),
            CommunityPostId = postId,
            UserProfileId = userProfile.Id
        });

        // Notifies the post owner when another user comments on their post.
        if (post.UserProfileId != userProfile.Id)
        {
            _context.Notifications.Add(new Notification
            {
                UserProfileId = post.UserProfileId,
                Message = $"{userProfile.FullName} commented on your community post: {post.Title}."
            });
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Feed));
    }

    [HttpGet]
    // Opens a community post for editing after ownership/admin permission is checked.
    public async Task<IActionResult> EditPost(int id)
    {
        var post = await _context.CommunityPosts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        // Only the post owner or an admin can open the edit page.
        if (!await CanManagePostAsync(post))
        {
            return Forbid();
        }

        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id, string title, string content, string postType)
    {
        var post = await _context.CommunityPosts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        // Protects posts from being modified by unauthorized users.
        if (!await CanManagePostAsync(post))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
        TempData["Error"] = "Title and content cannot be empty.";
            return View(post);
        }

        post.Title = title.Trim();
        post.Content = content.Trim();
        post.PostType = postType == "Help" ? "Help" : "Share";

        await _context.SaveChangesAsync();

        TempData["Message"] = "Post updated successfully.";
        return RedirectToAction(nameof(Feed));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Deletes a community post and its comments after permission is checked.
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _context.CommunityPosts
            .Include(p => p.User)
            .Include(p => p.PostComments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        // Confirms ownership or admin permission before deleting the post.
        if (!await CanManagePostAsync(post))
        {
            return Forbid();
        }

        // Removes related comments first to avoid orphaned child records.
        if (post.PostComments.Any())
        {
            _context.PostComments.RemoveRange(post.PostComments);
        }

        _context.CommunityPosts.Remove(post);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Post deleted successfully.";
        return RedirectToAction(nameof(Feed));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Deletes a comment when the user is the comment owner, post owner, or admin.
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await _context.PostComments
            .Include(c => c.User)
            .Include(c => c.Post)
                .ThenInclude(p => p!.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
        {
            return NotFound();
        }

        // Allows comment deletion by the comment owner, post owner, or admin.
        if (!await CanManageCommentAsync(comment))
        {
            return Forbid();
        }

        _context.PostComments.Remove(comment);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Comment deleted successfully.";
        return RedirectToAction(nameof(Feed));
    }

    private async Task<UserProfile?> GetCurrentProfileAsync()
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await GetPrimaryProfileForUserAsync(userId);
    }

    private async Task<UserProfile?> GetPrimaryProfileForUserAsync(string userId)
    {
        // Selects the most relevant profile for the current user.
        return await _context.UserProfiles
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsVerified)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    private async Task<bool> CanManagePostAsync(CommunityPost post)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var profile = await GetCurrentProfileAsync();

        if (profile == null)
        {
            return false;
        }

        // Loads the post owner when the related profile was not included in the query.
        if (post.User == null)
        {
            post.User = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.Id == post.UserProfileId);
        }

        return CanManagePost(post, profile, false);
    }

    private async Task<bool> CanManageCommentAsync(PostComment comment)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var profile = await GetCurrentProfileAsync();

        if (profile == null)
        {
            return false;
        }

        if (comment.UserProfileId == profile.Id)
        {
            return true;
        }

        if (comment.Post != null)
        {
            // Loads the post owner when needed for post-level permission checks.
            if (comment.Post.User == null)
            {
                comment.Post.User = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.Id == comment.Post.UserProfileId);
            }

            return CanManagePost(comment.Post, profile, false);
        }

        return false;
    }

    private static bool CanManagePost(CommunityPost post, UserProfile? currentProfile, bool isAdmin)
    {
        if (isAdmin)
        {
            return true;
        }

        if (currentProfile == null)
        {
            return false;
        }

        // Users can manage only their own posts.
        return post.UserProfileId == currentProfile.Id;
    }

    private static bool CanManageComment(PostComment comment, UserProfile? currentProfile, bool isAdmin)
    {
        if (isAdmin)
        {
            return true;
        }

        if (currentProfile == null)
        {
            return false;
        }

        // Comment owners can delete their own comments.
        if (comment.UserProfileId == currentProfile.Id)
        {
            return true;
        }

        // Post owners can also remove comments from their own posts.
        return comment.Post != null && CanManagePost(comment.Post, currentProfile, false);
    }
}
