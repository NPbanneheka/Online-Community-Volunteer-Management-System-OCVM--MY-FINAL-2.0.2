<<<<<<< HEAD
// Models/UserRating.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
=======
// Data model for UserRating.
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
=======
// This class defines structured data used by the application.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
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
