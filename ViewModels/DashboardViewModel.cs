namespace OCVMS.ViewModels;

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
