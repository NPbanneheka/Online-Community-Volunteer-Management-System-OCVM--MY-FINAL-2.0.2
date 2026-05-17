// ================================================================
// VIVA COMMENTED VERSION - Models/Notification.cs
// Purpose: Model file: represents one database entity/table and defines its fields plus navigation relationships.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCVMS.Models;
// This class defines structured data used by the application.
public class Notification : BaseEntity
{
    // string වෙනුවට int ලෙස වෙනස් කර ඇත
    public int UserProfileId { get; set; }

    [ForeignKey("UserProfileId")]
    public virtual UserProfile? User { get; set; }

    [Required, StringLength(255)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;
}