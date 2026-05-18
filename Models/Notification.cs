// Models/Notification.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Models;

// Model class: represents an OCVMS business object and is commonly mapped to a database table.
public class Notification : BaseEntity
{
    // string වෙනුවට int ලෙස වෙනස් කර ඇත
    // Foreign key linking this record to a UserProfile entity.
    public int UserProfileId { get; set; }

    [ForeignKey("UserProfileId")]
    public virtual UserProfile? User { get; set; }

    [Required, StringLength(255)]
    // Notification or feedback text shown to the user.
    public string Message { get; set; } = string.Empty;

    // Indicates whether a notification has already been read.

    public bool IsRead { get; set; } = false;
}
