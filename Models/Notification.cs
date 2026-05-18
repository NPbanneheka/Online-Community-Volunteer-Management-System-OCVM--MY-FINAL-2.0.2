<<<<<<< HEAD
// Models/Notification.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
=======
// Data model for Notification.
// Technology map:
// - EF Core uses this class to create/query a database table or relationship.
// - Properties become table columns; navigation properties connect related tables.
// Connected files: ApplicationDbContext configures this model; Controllers and Views use it.

>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Models;
<<<<<<< HEAD

// Model class: represents an OCVMS business object and is commonly mapped to a database table.
=======
// This class defines structured data used by the application.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
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
