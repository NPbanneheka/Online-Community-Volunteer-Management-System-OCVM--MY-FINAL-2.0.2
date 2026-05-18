<<<<<<< HEAD
// Models/UserProfile.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
=======
// Data model for UserProfile.
// Technology map:
// - EF Core uses this class to create/query a database table or relationship.
// - Properties become table columns; navigation properties connect related tables.
// Connected files: ApplicationDbContext configures this model; Controllers and Views use it.

>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
using System.ComponentModel.DataAnnotations;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Models;
<<<<<<< HEAD

// Model class: represents an OCVMS business object and is commonly mapped to a database table.
public class UserProfile : BaseEntity
{
    [Required]
    // Foreign key/string link to the ASP.NET Identity user account.
    public string UserId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Full Name is required.")]
    // Display name used in profiles, posts, registrations, and admin pages.
=======
// This class defines structured data used by the application.
public class UserProfile : BaseEntity
{
    [Required]
    // Links this record to the ASP.NET Identity user account.
    public string UserId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Full Name is required.")]
    // Readable user name shown in profile and admin screens.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
    public string FullName { get; set; } = string.Empty;
    
    // Stores the OCVMS role name, usually Admin, Organizer, or Volunteer.
    
    public string RoleName { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    // Email address shown or stored as part of the OCVMS user profile.
    public string? PublicEmail { get; set; }
    
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    // Phone/contact information stored in the user profile.
    public string? ContactNumber { get; set; }
    
    // Short profile description written by the user.
    
    public string? Bio { get; set; }
    // Volunteer skills used to describe abilities or interests.
    public string? Skills { get; set; }
    // Volunteer availability information shown in profile pages.
    public string? Availability { get; set; }
    // Path or URL of the user profile image.
    public string? ProfileImageUrl { get; set; }
<<<<<<< HEAD
    // Stores the organization name for organizer profiles and ownership checks.
    public string? OrganizationName { get; set; }
    // Indicates whether an admin has verified the user profile.
=======
    // Organizer organization name used for ownership and grouping.
    public string? OrganizationName { get; set; }
    // Admin-controlled verification flag.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
    public bool IsVerified { get; set; } = false;

    // --- NAVIGATION PROPERTIES ---
    public virtual ICollection<CommunityPost> Posts { get; set; } = new List<CommunityPost>();
    public virtual ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
    public virtual ICollection<HelpRequest> HelpRequests { get; set; } = new List<HelpRequest>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<VolunteerEvent> OrganizedEvents { get; set; } = new List<VolunteerEvent>();
}
