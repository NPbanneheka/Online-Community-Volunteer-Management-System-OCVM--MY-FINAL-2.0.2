// Data model for ErrorViewModel.
// Technology map:
// - EF Core uses this class to create/query a database table or relationship.
// - Properties become table columns; navigation properties connect related tables.
// Connected files: ApplicationDbContext configures this model; Controllers and Views use it.

namespace OCVMS.Models;

// පද්ධතියේ වැරදීමක් (Error) වූ විට එම විස්තර ගබඩා කර පෙන්වීමට මෙය භාවිතා වේ
// This class defines structured data used by the application.
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
