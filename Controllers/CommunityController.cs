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
        var posts = await _context.CommunityPosts.OrderByDescending(x => x.CreatedAt).ToListAsync();
        return View(posts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost(string title, string content)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(content))
        {
            _context.CommunityPosts.Add(new CommunityPost { Title = title, Content = content, UserId = user.Id });
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Feed));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int postId, string content)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (!string.IsNullOrWhiteSpace(content))
        {
            _context.PostComments.Add(new PostComment { CommunityPostId = postId, UserId = user.Id, Content = content });
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Feed));
    }
}
