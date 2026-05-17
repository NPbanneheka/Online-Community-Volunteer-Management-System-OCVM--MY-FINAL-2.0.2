// ================================================================
// VIVA COMMENTED VERSION - Models/UserRating.cs
// Purpose: Model file: represents one database entity/table and defines its fields plus navigation relationships.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

using System.ComponentModel.DataAnnotations;

namespace OCVMS.Models;
// This class defines structured data used by the application.
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
