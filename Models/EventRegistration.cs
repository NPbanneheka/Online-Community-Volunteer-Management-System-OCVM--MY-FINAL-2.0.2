// Data model for EventRegistration.
// Technology map:
// - EF Core uses this class to create/query a database table or relationship.
// - Properties become table columns; navigation properties connect related tables.
// Connected files: ApplicationDbContext configures this model; Controllers and Views use it.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace OCVMS.Models;
// This class defines structured data used by the application.
public class EventRegistration : BaseEntity
{
    // ඉවෙන්ට් එකේ ID එක
    [Required]
    public int VolunteerEventId { get; set; }

    // ඉවෙන්ට් එක සමඟ සම්බන්ධය (Navigation Property)
    [ForeignKey("VolunteerEventId")]
    public virtual VolunteerEvent? VolunteerEvent { get; set; }

    // රෙජිස්ටර් වන පරිශීලකයාගේ ID එක
    [Required]
    // Links this record to the ASP.NET Identity user account.
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public virtual IdentityUser? User { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.Now;
}
