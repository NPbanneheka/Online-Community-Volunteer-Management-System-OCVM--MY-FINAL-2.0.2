// Models/EventRegistration.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ASP.NET Core Identity services for users, roles, passwords, sign-in sessions, and lockout/ban behavior.
using Microsoft.AspNetCore.Identity;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Models;

// Model class: represents an OCVMS business object and is commonly mapped to a database table.
public class EventRegistration : BaseEntity
{
    // ඉවෙන්ට් එකේ ID එක
    [Required]
    // Foreign key linking a registration or rating to a volunteer event.
    public int VolunteerEventId { get; set; }

    // ඉවෙන්ට් එක සමඟ සම්බන්ධය (Navigation Property)
    [ForeignKey("VolunteerEventId")]
    public virtual VolunteerEvent? VolunteerEvent { get; set; }

    // රෙජිස්ටර් වන පරිශීලකයාගේ ID එක
    [Required]
    // Foreign key/string link to the ASP.NET Identity user account.
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public virtual IdentityUser? User { get; set; }

    // Stores the RegistrationDate value used by the application, database, or Razor view.

    public DateTime RegistrationDate { get; set; } = DateTime.Now;
}
