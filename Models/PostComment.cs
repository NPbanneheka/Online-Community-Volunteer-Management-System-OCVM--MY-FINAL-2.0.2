using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCVMS.Models;

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