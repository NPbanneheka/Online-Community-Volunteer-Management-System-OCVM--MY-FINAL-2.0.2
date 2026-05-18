<<<<<<< HEAD
// Models/VolunteerEvent.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
=======
// Data model for VolunteerEvent.
// Technology map:
// - EF Core uses this class to create/query a database table or relationship.
// - Properties become table columns; navigation properties connect related tables.
// Connected files: ApplicationDbContext configures this model; Controllers and Views use it.

>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Models;
<<<<<<< HEAD

// Model class: represents an OCVMS business object and is commonly mapped to a database table.
=======
// This class defines structured data used by the application.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
public class VolunteerEvent : BaseEntity
{
    [Required(ErrorMessage = "Event Title is required.")]
    [StringLength(200)]
<<<<<<< HEAD
    // Main title shown to users in event, post, or request pages.
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event Description is required.")]
    // Longer explanation or details shown in the UI.
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location is required.")]
    // Physical or descriptive location displayed for the event.
=======
    // Main title shown in the UI.
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event Description is required.")]
    // Detailed explanation shown to users.
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location is required.")]
    // Physical/event location displayed to volunteers.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event Date is required.")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
<<<<<<< HEAD
    // Date on which the volunteer event is scheduled.
=======
    // Date of the volunteer event.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
    public DateTime EventDate { get; set; }

    [Required(ErrorMessage = "Event Time is required.")]
    [DataType(DataType.Time)]
<<<<<<< HEAD
    // Time on which the volunteer event is scheduled.
=======
    // Time of the volunteer event.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
    public TimeSpan EventTime { get; set; }

    [Display(Name = "Registration Open Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
<<<<<<< HEAD
    // Date when volunteers are allowed to start registering for the event.
=======
    // Date from which volunteers are allowed to register.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
    public DateTime RegistrationOpenDate { get; set; } = DateTime.Today;

    [Display(Name = "Registration Closing Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
<<<<<<< HEAD
    // Date after which event registration is closed.
=======
    // Final date for volunteer registration.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
    public DateTime? RegistrationClosingDate { get; set; }

    [Required(ErrorMessage = "Capacity is required.")]
    [Range(1, 10000, ErrorMessage = "Capacity must be at least 1.")]
    // Maximum number of volunteers allowed for the event.
    public int Capacity { get; set; }

<<<<<<< HEAD
    // Current workflow state such as active, closed, pending, or resolved.

=======
    // Current workflow state used for filtering and decisions.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
    public string Status { get; set; } = "Upcoming";
    // Path or URL of an uploaded/displayed image used by the UI.
    public string? ImageUrl { get; set; }

    // Foreign key linking an event to the organizer profile that created it.

    public int OrganizerProfileId { get; set; }

    [ForeignKey("OrganizerProfileId")]
    [ValidateNever]
    public virtual UserProfile? OrganizerProfile { get; set; }

    [ValidateNever]
    public virtual ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();

    [NotMapped]
    // Computed property used by the UI; it is calculated from other values instead of being manually stored.
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
    // Computed property used by the UI; it is calculated from other values instead of being manually stored.
    public bool IsRegistrationOpen => string.Equals(PublicStatus, "Open", StringComparison.OrdinalIgnoreCase);
}
