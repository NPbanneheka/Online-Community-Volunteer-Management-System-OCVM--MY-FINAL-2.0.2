// Data model for CommunityPost.
// Technology map:
// - EF Core uses this class to create/query a database table or relationship.
// - Properties become table columns; navigation properties connect related tables.
// Connected files: ApplicationDbContext configures this model; Controllers and Views use it.

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
