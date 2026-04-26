namespace OCVMS.Models;

// පද්ධතියේ වැරදීමක් (Error) වූ විට එම විස්තර ගබඩා කර පෙන්වීමට මෙය භාවිතා වේ
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}