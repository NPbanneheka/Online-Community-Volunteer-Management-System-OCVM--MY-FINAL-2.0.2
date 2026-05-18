// Models/ErrorViewModel.cs
// This entity/model file that represents application data and database structure.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Models;

// පද්ධතියේ වැරදීමක් (Error) වූ විට එම විස්තර ගබඩා කර පෙන්වීමට මෙය භාවිතා වේ
// ViewModel class: contains only the data needed by a form or page, often with validation rules.
public class ErrorViewModel
{
    // Request identifier displayed on the error page for troubleshooting.
    public string? RequestId { get; set; }

    // Computed property used by the UI; it is calculated from other values instead of being manually stored.

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
