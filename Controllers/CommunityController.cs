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
}
