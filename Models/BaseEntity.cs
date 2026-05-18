// Data model for BaseEntity.
// Technology map:
// - EF Core uses this class to create/query a database table or relationship.
// - Properties become table columns; navigation properties connect related tables.
// Connected files: ApplicationDbContext configures this model; Controllers and Views use it.

namespace OCVMS.Models;

public abstract class BaseEntity
{
    // Primary key used to uniquely identify this record.
    public int Id { get; set; }
    // Stores when this record was created.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
