// ================================================================
// VIVA COMMENTED VERSION - Models/EventRegistration.cs
// Purpose: Model file: represents one database entity/table and defines its fields plus navigation relationships.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

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