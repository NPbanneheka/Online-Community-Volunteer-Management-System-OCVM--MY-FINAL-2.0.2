// Data model for HelpRequest.
// Technology map:
// - EF Core uses this class to create/query a database table or relationship.
// - Properties become table columns; navigation properties connect related tables.
// Connected files: ApplicationDbContext configures this model; Controllers and Views use it.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCVMS.Models;
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
    public string Status { get; set; } = "Pending";

    // int ලෙස තිබිය යුතුය
    public int UserProfileId { get; set; }

    [ForeignKey("UserProfileId")]
    public virtual UserProfile? SubmittedBy { get; set; }
}
