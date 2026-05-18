<<<<<<< HEAD
// ViewModels/DashboardViewModel.cs
// This view model file that carries validated form or dashboard data between controllers and Razor views.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.ViewModels;

// ViewModel class: contains only the data needed by a form or page, often with validation rules.
=======
// View model for DashboardViewModel.
// Technology map:
// - ASP.NET Core MVC uses this class to transfer form/page data between Controller and Razor View.
// - DataAnnotation attributes provide validation rules shown in the UI.
// Connected files: Controllers receive this model; Views bind form fields to these properties.

namespace OCVMS.ViewModels;
// This class defines structured data used by the application.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
public class DashboardViewModel
{
    // Stores the TotalEvents value used by the application, database, or Razor view.
    public int TotalEvents { get; set; }
    // Stores the ActiveEvents value used by the application, database, or Razor view.
    public int ActiveEvents { get; set; }
    // Stores the MyRegistrations value used by the application, database, or Razor view.
    public int MyRegistrations { get; set; }
    // Stores the CommunityPosts value used by the application, database, or Razor view.
    public int CommunityPosts { get; set; }
    // Stores the CommunityHelpPosts value used by the application, database, or Razor view.
    public int CommunityHelpPosts { get; set; }
    // Stores the AverageRating value used by the application, database, or Razor view.
    public double AverageRating { get; set; }
    // Stores the RatingCount value used by the application, database, or Razor view.
    public int RatingCount { get; set; }
}
