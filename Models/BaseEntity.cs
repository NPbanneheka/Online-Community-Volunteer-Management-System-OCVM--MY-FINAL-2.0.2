// Models/BaseEntity.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Models;

public abstract class BaseEntity
{
    // Primary key identifier for this record in the database.
    public int Id { get; set; }
    // Timestamp used for sorting and showing when the record was created.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
