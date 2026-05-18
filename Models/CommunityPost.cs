<<<<<<< HEAD
// Models/CommunityPost.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
=======
// Data model for CommunityPost.
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
public class CommunityPost : BaseEntity
{
    [Required]
    // Main title shown to users in event, post, or request pages.
=======
// This class defines structured data used by the application.
public class CommunityPost : BaseEntity
{
    [Required]
    // Main title shown in the UI.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
    public string Title { get; set; } = string.Empty;

    [Required]
    // Main text body for community posts or comments.
    public string Content { get; set; } = string.Empty;

    // Classifies community content, such as Help or Share.

    public string PostType { get; set; } = "Share"; 

    [Required]
    // Foreign key linking this record to a UserProfile entity.
    public int UserProfileId { get; set; }

    [ForeignKey("UserProfileId")]
    public virtual UserProfile? User { get; set; }

    // අලුතින් එක් කළ කොටස: පෝස්ට් එකට අදාළ කමෙන්ට්ස් ගබඩා කර තබා ගැනීමට
    public virtual ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();
}
