// ================================================================
// VIVA COMMENTED VERSION - Models/PostComment.cs
// Purpose: Model file: represents one database entity/table and defines its fields plus navigation relationships.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCVMS.Models;
// This class defines structured data used by the application.
public class PostComment : BaseEntity
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public int CommunityPostId { get; set; }

    [ForeignKey("CommunityPostId")]
    public virtual CommunityPost? Post { get; set; }

    [Required]
    public int UserProfileId { get; set; }

    [ForeignKey("UserProfileId")]
    public virtual UserProfile? User { get; set; }
}