using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCVMS.Models;

public class CommunityPost : BaseEntity
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public string PostType { get; set; } = "Share"; 

    [Required]
    public int UserProfileId { get; set; }

    [ForeignKey("UserProfileId")]
    public virtual UserProfile? User { get; set; }

    // අලුතින් එක් කළ කොටස: පෝස්ට් එකට අදාළ කමෙන්ට්ස් ගබඩා කර තබා ගැනීමට
    public virtual ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();
}