using System.ComponentModel.DataAnnotations;

namespace OCVMS.Models;

public class CommunityPost : BaseEntity
{
    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    public string Content { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public int? EventId { get; set; }

    public int LikeCount { get; set; }
}
