using System.ComponentModel.DataAnnotations;

namespace OCVMS.Models;

public class PostComment : BaseEntity
{
    public int CommunityPostId { get; set; }
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Content { get; set; } = string.Empty;
}
