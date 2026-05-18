// Models/CommunityPost.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Models;

// Model class: represents an OCVMS business object and is commonly mapped to a database table.
public class CommunityPost : BaseEntity
{
    [Required]
    // Main title shown to users in event, post, or request pages.
    public string Title { get; set; } = string.Empty;

    [Required]
    // Main text body for community posts or comments.
    public string Content { get; set; } = string.Empty;

    // Classifies community content, such as Help or Share.

    public string PostType { get; set; } = "Share"; 

    [Required]
    // Foreign key linking this record to a UserProfile entity.
    public int UserProfileId { get; set; }

    [ForeignKey("UserProfileId")]
    public virtual UserProfile? User { get; set; }

    // අලුතින් එක් කළ කොටස: පෝස්ට් එකට අදාළ කමෙන්ට්ස් ගබඩා කර තබා ගැනීමට
    public virtual ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();
}
