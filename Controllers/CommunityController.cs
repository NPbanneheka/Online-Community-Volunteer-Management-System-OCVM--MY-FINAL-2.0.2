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

    public async Task<IActionResult> Feed()
    {
        var posts = await _context.CommunityPosts
            .Include(p => p.User)
            .Include(p => p.PostComments)
                .ThenInclude(c => c.User)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var identityUserId = _userManager.GetUserId(User);
        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == identityUserId);

        ViewBag.CurrentProfileId = userProfile?.Id;
        ViewBag.IsAdmin = User.IsInRole("Admin");
        ViewBag.PostCount = userProfile != null ? posts.Count(p => p.UserProfileId == userProfile.Id) : 0;

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

        var identityUserId = _userManager.GetUserId(User);
        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == identityUserId);

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

        var identityUserId = _userManager.GetUserId(User);
        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == identityUserId);

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
        var post = await _context.CommunityPosts.FirstOrDefaultAsync(p => p.Id == id);
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
        var post = await _context.CommunityPosts.FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return NotFound();

        if (!await CanManagePostAsync(post))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            TempData["Error"] = "Title and content cannot be empty.";
            return RedirectToAction(nameof(Feed));
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
            .Include(c => c.Post)
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

    private async Task<bool> CanManagePostAsync(CommunityPost post)
    {
        if (User.IsInRole("Admin")) return true;

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        return profile != null && post.UserProfileId == profile.Id;
    }

    private async Task<bool> CanManageCommentAsync(PostComment comment)
    {
        if (User.IsInRole("Admin")) return true;

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        if (profile == null) return false;

        return comment.UserProfileId == profile.Id
            || (comment.Post != null && comment.Post.UserProfileId == profile.Id);
    }
}
