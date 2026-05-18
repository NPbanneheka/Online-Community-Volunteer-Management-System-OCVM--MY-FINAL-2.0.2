// Data model for UserRating.
// Technology map:
// - EF Core uses this class to create/query a database table or relationship.
// - Properties become table columns; navigation properties connect related tables.
// Connected files: ApplicationDbContext configures this model; Controllers and Views use it.

using System.ComponentModel.DataAnnotations;

namespace OCVMS.Models;
// This class defines structured data used by the application.
public class UserRating : BaseEntity
{
    public string FromUserId { get; set; } = string.Empty;
    public string ToUserId { get; set; } = string.Empty;
    public int EventId { get; set; }

    [Range(1, 5)]
    public int Score { get; set; }

    [StringLength(400)]
    public string? ReviewText { get; set; }
}
