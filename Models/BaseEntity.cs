<<<<<<< HEAD
// Models/BaseEntity.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
=======
// Data model for BaseEntity.
// Technology map:
// - EF Core uses this class to create/query a database table or relationship.
// - Properties become table columns; navigation properties connect related tables.
// Connected files: ApplicationDbContext configures this model; Controllers and Views use it.

>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
namespace OCVMS.Models;

public abstract class BaseEntity
{
<<<<<<< HEAD
    // Primary key identifier for this record in the database.
    public int Id { get; set; }
    // Timestamp used for sorting and showing when the record was created.
=======
    // Primary key used to uniquely identify this record.
    public int Id { get; set; }
    // Stores when this record was created.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
