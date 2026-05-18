// Data model for Notification.
// Technology map:
// - EF Core uses this class to create/query a database table or relationship.
// - Properties become table columns; navigation properties connect related tables.
// Connected files: ApplicationDbContext configures this model; Controllers and Views use it.

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
