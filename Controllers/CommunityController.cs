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
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CommunityController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Feed(string? filter, string? sortOrder)
    {
        filter = string.IsNullOrWhiteSpace(filter) ? "All" : filter;
        sortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "Newest" : sortOrder;

        var postsQuery = _context.CommunityPosts
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

        await _context.SaveChangesAsync();

        TempData["Message"] = "Post published successfully!";
        return RedirectToAction(nameof(Feed));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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

    private async Task<UserProfile?> GetCurrentProfileAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return null;
        return await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
    }

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
        if (post.UserProfileId == currentProfile.Id) return true;

        return IsOrganizerInSameOrganization(currentProfile, post.User);
    }

    private static bool CanManageComment(PostComment comment, UserProfile? currentProfile, bool isAdmin)
    {
        if (isAdmin) return true;
        if (currentProfile == null) return false;
        if (comment.UserProfileId == currentProfile.Id) return true;
        return comment.Post != null && CanManagePost(comment.Post, currentProfile, false);
    }

    private static bool IsOrganizerInSameOrganization(UserProfile currentProfile, UserProfile? ownerProfile)
    {
        if (ownerProfile == null) return false;
        if (currentProfile.RoleName != "Organizer" || ownerProfile.RoleName != "Organizer") return false;
        if (string.IsNullOrWhiteSpace(currentProfile.OrganizationName) || string.IsNullOrWhiteSpace(ownerProfile.OrganizationName)) return false;

        return string.Equals(
            currentProfile.OrganizationName.Trim(),
            ownerProfile.OrganizationName.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }
}
