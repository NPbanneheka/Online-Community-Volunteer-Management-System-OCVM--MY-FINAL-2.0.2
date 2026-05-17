// ================================================================
// VIVA COMMENTED VERSION - Models/HelpRequest.cs
// Purpose: Model file: represents one database entity/table and defines its fields plus navigation relationships.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

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