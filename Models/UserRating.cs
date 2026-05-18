// Models/UserRating.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
using System.ComponentModel.DataAnnotations;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Models;

// Model class: represents an OCVMS business object and is commonly mapped to a database table.
public class UserRating : BaseEntity
{
    // Stores the FromUserId value used by the application, database, or Razor view.
    public string FromUserId { get; set; } = string.Empty;
    // Stores the ToUserId value used by the application, database, or Razor view.
    public string ToUserId { get; set; } = string.Empty;
    // Stores the EventId value used by the application, database, or Razor view.
    public int EventId { get; set; }

    [Range(1, 5)]
    // Stores the Score value used by the application, database, or Razor view.
    public int Score { get; set; }

    [StringLength(400)]
    // Stores the ReviewText value used by the application, database, or Razor view.
    public string? ReviewText { get; set; }
}
