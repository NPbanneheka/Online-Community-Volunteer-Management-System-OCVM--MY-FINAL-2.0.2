// Models/HelpRequest.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Models;

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

    public string Status { get; set; } = "Pending";

    // int ලෙස තිබිය යුතුය
    // Foreign key linking this record to a UserProfile entity.
    public int UserProfileId { get; set; }

    [ForeignKey("UserProfileId")]
    public virtual UserProfile? SubmittedBy { get; set; }
}
