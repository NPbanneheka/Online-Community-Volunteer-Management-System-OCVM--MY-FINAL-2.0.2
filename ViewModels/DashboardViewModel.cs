// ================================================================
// VIVA COMMENTED VERSION - ViewModels/DashboardViewModel.cs
// Purpose: ViewModel file: carries validated data between Razor forms/views and controller actions without exposing full database entities.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

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
