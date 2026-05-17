// ================================================================
// VIVA COMMENTED VERSION - ViewModels/ProfileEditViewModel.cs
// Purpose: ViewModel file: carries validated data between Razor forms/views and controller actions without exposing full database entities.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // පින්තූර අප්ලෝඩ් (IFormFile) සඳහා මෙය අත්‍යවශ්‍ය වේ

namespace OCVMS.ViewModels;
// This class defines structured data used by the application.
public class ProfileEditViewModel
{
    // --- View එකේ තිබුණු Hidden Field දත්ත සඳහා අත්‍යවශ්‍ය කොටස් ---
    // Primary key used to uniquely identify this record.
    public int Id { get; set; }
    // Links this record to the ASP.NET Identity user account.
    public string UserId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }

    // --- සාමාන්‍ය ෆෝම් දත්ත (ඉංග්‍රීසි එරර්ස් සමඟ) ---
    [Required(ErrorMessage = "Full Name is required.")]
    [Display(Name = "Full Name")]
    // Readable user name shown in profile and admin screens.
    public string FullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Public Email")]
    public string? PublicEmail { get; set; }

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [Display(Name = "Contact Number")]
    public string? ContactNumber { get; set; }

    public string? Availability { get; set; }
    
    public string? Skills { get; set; }
    
    [Display(Name = "Organization Name")]
    // Organizer organization name used for ownership and grouping.
    public string? OrganizationName { get; set; }
    
    public string? Bio { get; set; }

    // --- අලුත් පින්තූරය අප්ලෝඩ් කිරීමට අවශ්‍ය කොටස ---
    [Display(Name = "Upload New Profile Photo")]
    public IFormFile? ProfileImage { get; set; }
}