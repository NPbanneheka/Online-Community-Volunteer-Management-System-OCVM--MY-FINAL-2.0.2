// ================================================================
// VIVA COMMENTED VERSION - Controllers/CommunityController.cs
// Purpose: Handles community posts and comments so users can share updates and discussions.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

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
    // Dependencies injected through constructor for database, identity, hosting, or logging work.
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CommunityController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Controller action: handles a request, performs validation/business logic, and returns a response/view.

    public async Task<IActionResult> Feed(string? filter, string? sortOrder)
    {
        filter = string.IsNullOrWhiteSpace(filter) ? "All" : filter;
        sortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "Newest" : sortOrder;

        var postsQuery = _context.CommunityPosts
            // Include loads related table data needed by the view.
            .Include(p => p.User)
            .Include(p => p.PostComments)
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

        var posts = await postsQuery.ToListAsync();
        var allPosts = await _context.CommunityPosts.ToListAsync();

        var currentProfile = await GetCurrentProfileAsync();
        var isAdmin = User.IsInRole("Admin");

        // ViewBag passes small extra values to the Razor view.
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

        return View(posts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Controller action: handles a request, performs validation/business logic, and returns a response/view.
    public async Task<IActionResult> CreatePost(string title, string content, string postType)
    {
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

        postType = postType == "Help" ? "Help" : "Share";

        var post = new CommunityPost
        {
            Title = title.Trim(),
            Content = content.Trim(),
            PostType = postType,
            UserProfileId = userProfile.Id
        };

        _context.CommunityPosts.Add(post);

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

        // Save all pending database changes.
        await _context.SaveChangesAsync();

        // TempData message is shown once after redirect.
            TempData["Message"] = "Post published successfully!";
        return RedirectToAction(nameof(Feed));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Controller action: handles a request, performs validation/business logic, and returns a response/view.
    public async Task<IActionResult> AddComment(int postId, string commentContent)
    {
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
    // Controller action: handles a request, performs validation/business logic, and returns a response/view.
    public async Task<IActionResult> EditPost(int id)
    {
        var post = await _context.CommunityPosts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return NotFound();

        if (!await CanManagePostAsync(post))
        {
            return Forbid();
        }

        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Controller action: handles a request, performs validation/business logic, and returns a response/view.
    public async Task<IActionResult> EditPost(int id, string title, string content, string postType)
    {
        var post = await _context.CommunityPosts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return NotFound();

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
    // Controller action: handles a request, performs validation/business logic, and returns a response/view.
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _context.CommunityPosts
            .Include(p => p.User)
            .Include(p => p.PostComments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();

        if (!await CanManagePostAsync(post))
        {
            return Forbid();
        }

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
    // Controller action: handles a request, performs validation/business logic, and returns a response/view.
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await _context.PostComments
            .Include(c => c.User)
            .Include(c => c.Post)
                .ThenInclude(p => p!.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null) return NotFound();

        if (!await CanManageCommentAsync(comment))
        {
            return Forbid();
        }

        _context.PostComments.Remove(comment);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Comment deleted successfully.";
        return RedirectToAction(nameof(Feed));
    }

    // Helper method: keeps repeated controller logic in one reusable place.

    private async Task<UserProfile?> GetCurrentProfileAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return null;
        return await GetPrimaryProfileForUserAsync(userId);
    }

    // Helper method: keeps repeated controller logic in one reusable place.

    private async Task<UserProfile?> GetPrimaryProfileForUserAsync(string userId)
    {
        return await _context.UserProfiles
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsVerified)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    // Helper method: keeps repeated controller logic in one reusable place.

    private async Task<bool> CanManagePostAsync(CommunityPost post)
    {
        if (User.IsInRole("Admin")) return true;

        var profile = await GetCurrentProfileAsync();
        if (profile == null) return false;

        if (post.User == null)
        {
            post.User = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == post.UserProfileId);
        }

        return CanManagePost(post, profile, false);
    }

    // Helper method: keeps repeated controller logic in one reusable place.

    private async Task<bool> CanManageCommentAsync(PostComment comment)
    {
        if (User.IsInRole("Admin")) return true;

        var profile = await GetCurrentProfileAsync();
        if (profile == null) return false;

        if (comment.UserProfileId == profile.Id) return true;

        if (comment.Post != null)
        {
            if (comment.Post.User == null)
            {
                comment.Post.User = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == comment.Post.UserProfileId);
            }

            return CanManagePost(comment.Post, profile, false);
        }

        return false;
    }

    private static bool CanManagePost(CommunityPost post, UserProfile? currentProfile, bool isAdmin)
    {
        if (isAdmin) return true;
        if (currentProfile == null) return false;

        // Final ownership rule: users can edit/delete only their own posts. Admin can manage all.
        return post.UserProfileId == currentProfile.Id;
    }

    private static bool CanManageComment(PostComment comment, UserProfile? currentProfile, bool isAdmin)
    {
        if (isAdmin) return true;
        if (currentProfile == null) return false;

        // Comment owner can delete their own comment. The post owner can also remove comments from their own post.
        if (comment.UserProfileId == currentProfile.Id) return true;
        return comment.Post != null && CanManagePost(comment.Post, currentProfile, false);
    }
}
