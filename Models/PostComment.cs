// Data model for PostComment.
// Technology map:
// - EF Core uses this class to create/query a database table or relationship.
// - Properties become table columns; navigation properties connect related tables.
// Connected files: ApplicationDbContext configures this model; Controllers and Views use it.

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
