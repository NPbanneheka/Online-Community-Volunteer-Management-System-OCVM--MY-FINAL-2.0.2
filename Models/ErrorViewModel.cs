// ================================================================
// VIVA COMMENTED VERSION - Models/ErrorViewModel.cs
// Purpose: Model file: represents one database entity/table and defines its fields plus navigation relationships.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

namespace OCVMS.Models;

// පද්ධතියේ වැරදීමක් (Error) වූ විට එම විස්තර ගබඩා කර පෙන්වීමට මෙය භාවිතා වේ
// This class defines structured data used by the application.public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}