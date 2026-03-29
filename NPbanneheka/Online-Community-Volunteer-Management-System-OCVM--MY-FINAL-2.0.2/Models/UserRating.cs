using System.ComponentModel.DataAnnotations;

namespace OCVMS.Models;

public class UserRating : BaseEntity
{
    public string FromUserId { get; set; } = string.Empty;
    public string ToUserId { get; set; } = string.Empty;
    public int EventId { get; set; }

    [Range(1, 5)]
    public int Score { get; set; }

    [StringLength(400)]
    public string? ReviewText { get; set; }
}
