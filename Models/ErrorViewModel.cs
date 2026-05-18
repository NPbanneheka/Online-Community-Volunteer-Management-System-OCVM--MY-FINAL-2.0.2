<<<<<<< HEAD
// Models/ErrorViewModel.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Models;

// පද්ධතියේ වැරදීමක් (Error) වූ විට එම විස්තර ගබඩා කර පෙන්වීමට මෙය භාවිතා වේ
// ViewModel class: contains only the data needed by a form or page, often with validation rules.
=======
// Data model for ErrorViewModel.
// Technology map:
// - EF Core uses this class to create/query a database table or relationship.
// - Properties become table columns; navigation properties connect related tables.
// Connected files: ApplicationDbContext configures this model; Controllers and Views use it.

namespace OCVMS.Models;

// පද්ධතියේ වැරදීමක් (Error) වූ විට එම විස්තර ගබඩා කර පෙන්වීමට මෙය භාවිතා වේ
// This class defines structured data used by the application.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
public class ErrorViewModel
{
    // Request identifier displayed on the error page for troubleshooting.
    public string? RequestId { get; set; }

    // Computed property used by the UI; it is calculated from other values instead of being manually stored.

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
