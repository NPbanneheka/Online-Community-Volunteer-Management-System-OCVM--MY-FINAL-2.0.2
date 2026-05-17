// ================================================================
// VIVA COMMENTED VERSION - Models/CommunityPost.cs
// Purpose: Model file: represents one database entity/table and defines its fields plus navigation relationships.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCVMS.Models;
// This class defines structured data used by the application.
public class CommunityPost : BaseEntity
{
    [Required]
    // Main title shown in the UI.
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