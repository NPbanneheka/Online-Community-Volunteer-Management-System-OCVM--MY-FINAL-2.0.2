// View model for DashboardViewModel.
// Technology map:
// - ASP.NET Core MVC uses this class to transfer form/page data between Controller and Razor View.
// - DataAnnotation attributes provide validation rules shown in the UI.
// Connected files: Controllers receive this model; Views bind form fields to these properties.

namespace OCVMS.ViewModels;
// This class defines structured data used by the application.
public class DashboardViewModel
{
    public int TotalEvents { get; set; }
    public int ActiveEvents { get; set; }
    public int MyRegistrations { get; set; }
    public int CommunityPosts { get; set; }
    public int CommunityHelpPosts { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
}
