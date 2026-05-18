<<<<<<< HEAD
// Models/HelpRequest.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
=======
// Data model for HelpRequest.
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
public class HelpRequest : BaseEntity
{
    [Required, StringLength(100)]
    // Main title shown to users in event, post, or request pages.
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(500)]
    // Longer explanation or details shown in the UI.
    public string Description { get; set; } = string.Empty;

    // Current workflow state such as active, closed, pending, or resolved.

=======
// This class defines structured data used by the application.
public class HelpRequest : BaseEntity
{
    [Required, StringLength(100)]
    // Main title shown in the UI.
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(500)]
    // Detailed explanation shown to users.
    public string Description { get; set; } = string.Empty;

    // Current workflow state used for filtering and decisions.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
    public string Status { get; set; } = "Pending";

    // int ලෙස තිබිය යුතුය
    // Foreign key linking this record to a UserProfile entity.
    public int UserProfileId { get; set; }

    [ForeignKey("UserProfileId")]
    public virtual UserProfile? SubmittedBy { get; set; }
}
