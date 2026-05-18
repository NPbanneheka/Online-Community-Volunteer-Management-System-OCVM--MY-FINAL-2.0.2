// Data model for VolunteerEvent.
// Technology map:
// - EF Core uses this class to create/query a database table or relationship.
// - Properties become table columns; navigation properties connect related tables.
// Connected files: ApplicationDbContext configures this model; Controllers and Views use it.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OCVMS.Models;
// This class defines structured data used by the application.
public class VolunteerEvent : BaseEntity
{
    [Required(ErrorMessage = "Event Title is required.")]
    [StringLength(200)]
    // Main title shown in the UI.
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event Description is required.")]
    // Detailed explanation shown to users.
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location is required.")]
    // Physical/event location displayed to volunteers.
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event Date is required.")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    // Date of the volunteer event.
    public DateTime EventDate { get; set; }

    [Required(ErrorMessage = "Event Time is required.")]
    [DataType(DataType.Time)]
    // Time of the volunteer event.
    public TimeSpan EventTime { get; set; }

    [Display(Name = "Registration Open Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    // Date from which volunteers are allowed to register.
    public DateTime RegistrationOpenDate { get; set; } = DateTime.Today;

    [Display(Name = "Registration Closing Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    // Final date for volunteer registration.
    public DateTime? RegistrationClosingDate { get; set; }

    [Required(ErrorMessage = "Capacity is required.")]
    [Range(1, 10000, ErrorMessage = "Capacity must be at least 1.")]
    // Maximum number of volunteers allowed for the event.
    public int Capacity { get; set; }

    // Current workflow state used for filtering and decisions.
    public string Status { get; set; } = "Upcoming";
    public string? ImageUrl { get; set; }

    public int OrganizerProfileId { get; set; }

    [ForeignKey("OrganizerProfileId")]
    [ValidateNever]
    public virtual UserProfile? OrganizerProfile { get; set; }

    [ValidateNever]
    public virtual ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();

    [NotMapped]
    public bool IsStatusClosedLike =>
        string.Equals(Status, "Closed", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Status, "Completed", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Status, "Cancelled", StringComparison.OrdinalIgnoreCase);

    [NotMapped]
    public string PublicStatus
    {
        get
        {
            var today = DateTime.Today;

            if (string.Equals(Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return "Cancelled";
            }

            if (string.Equals(Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                return "Completed";
            }

            if (string.Equals(Status, "Closed", StringComparison.OrdinalIgnoreCase))
            {
                return "Closed";
            }

            if (EventDate.Date < today)
            {
                return "Closed";
            }

            if (RegistrationOpenDate.Date > today)
            {
                return "Upcoming";
            }

            if (RegistrationClosingDate.HasValue && RegistrationClosingDate.Value.Date < today)
            {
                return "Closed";
            }

            return "Open";
        }
    }

    [NotMapped]
    public bool IsRegistrationOpen => string.Equals(PublicStatus, "Open", StringComparison.OrdinalIgnoreCase);
}
