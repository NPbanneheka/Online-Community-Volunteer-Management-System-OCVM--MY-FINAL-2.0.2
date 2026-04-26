using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCVMS.Models;

public class HelpRequest : BaseEntity
{
    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    // int ලෙස තිබිය යුතුය
    public int UserProfileId { get; set; }

    [ForeignKey("UserProfileId")]
    public virtual UserProfile? SubmittedBy { get; set; }
}