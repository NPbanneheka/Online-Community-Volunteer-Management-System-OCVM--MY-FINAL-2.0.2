// ================================================================
// VIVA COMMENTED VERSION - Models/BaseEntity.cs
// Purpose: Model file: represents one database entity/table and defines its fields plus navigation relationships.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

namespace OCVMS.Models;

public abstract class BaseEntity
{
    // Primary key used to uniquely identify this record.
    public int Id { get; set; }
    // Stores when this record was created.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
