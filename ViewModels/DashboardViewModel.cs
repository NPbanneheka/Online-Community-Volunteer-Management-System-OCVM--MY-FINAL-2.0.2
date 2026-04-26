namespace OCVMS.ViewModels;

/// <summary>
/// Dashboard එකේ පෙන්වන දත්ත එකතු කර තබා ගන්නා පැකට්ටුව.
/// </summary>
public class DashboardViewModel
{
    public int TotalEvents { get; set; }      // පද්ධතියේ ඇති මුළු ඉවෙන්ට් ගණන
    public int MyRegistrations { get; set; }  // මම සහභාගී වන ඉවෙන්ට් ගණන
    public int CommunityPosts { get; set; }   // පද්ධතියේ ඇති පෝස්ට් ගණන
    public double AverageRating { get; set; } // මට ලැබී ඇති සාමාන්‍ය රේටින්ග් අගය
}