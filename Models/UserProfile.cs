// ================================================================
// VIVA COMMENTED VERSION - Models/UserProfile.cs
// Purpose: Model file: represents one database entity/table and defines its fields plus navigation relationships.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

using System.ComponentModel.DataAnnotations;

namespace OCVMS.Models;
// This class defines structured data used by the application.
public class UserProfile : BaseEntity
{
    [Required]
    // Links this record to the ASP.NET Identity user account.
    public string UserId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Full Name is required.")]
    // Readable user name shown in profile and admin screens.
    public string FullName { get; set; } = string.Empty;
    
    public string RoleName { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string? PublicEmail { get; set; }
    
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public string? ContactNumber { get; set; }
    
    public string? Bio { get; set; }
    public string? Skills { get; set; }
    public string? Availability { get; set; }
    public string? ProfileImageUrl { get; set; }
    // Organizer organization name used for ownership and grouping.
    public string? OrganizationName { get; set; }
    // Admin-controlled verification flag.
    public bool IsVerified { get; set; } = false;

    // --- NAVIGATION PROPERTIES ---
    public virtual ICollection<CommunityPost> Posts { get; set; } = new List<CommunityPost>();
    public virtual ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
    public virtual ICollection<HelpRequest> HelpRequests { get; set; } = new List<HelpRequest>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<VolunteerEvent> OrganizedEvents { get; set; } = new List<VolunteerEvent>();
}