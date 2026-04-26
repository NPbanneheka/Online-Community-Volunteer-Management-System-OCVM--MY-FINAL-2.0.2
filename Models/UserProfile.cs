using System.ComponentModel.DataAnnotations;

namespace OCVMS.Models;

public class UserProfile : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Full Name is required.")]
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
    public string? OrganizationName { get; set; }
    public bool IsVerified { get; set; } = false;

    // --- NAVIGATION PROPERTIES ---
    public virtual ICollection<CommunityPost> Posts { get; set; } = new List<CommunityPost>();
    public virtual ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
    public virtual ICollection<HelpRequest> HelpRequests { get; set; } = new List<HelpRequest>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<VolunteerEvent> OrganizedEvents { get; set; } = new List<VolunteerEvent>();
}