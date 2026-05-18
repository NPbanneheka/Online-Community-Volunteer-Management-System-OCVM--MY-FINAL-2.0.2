// Models/PostComment.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Models;

// Model class: represents an OCVMS business object and is commonly mapped to a database table.
public class PostComment : BaseEntity
{
    [Required]
    // Main text body for community posts or comments.
    public string Content { get; set; } = string.Empty;

    [Required]
    // Stores the CommunityPostId value used by the application, database, or Razor view.
    public int CommunityPostId { get; set; }

    [ForeignKey("CommunityPostId")]
    public virtual CommunityPost? Post { get; set; }

    [Required]
    // Foreign key linking this record to a UserProfile entity.
    public int UserProfileId { get; set; }

    [ForeignKey("UserProfileId")]
    public virtual UserProfile? User { get; set; }
}
