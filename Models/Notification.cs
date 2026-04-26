using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCVMS.Models;

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