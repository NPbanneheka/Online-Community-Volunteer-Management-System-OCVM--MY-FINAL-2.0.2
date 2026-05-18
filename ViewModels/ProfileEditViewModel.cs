// ViewModels/ProfileEditViewModel.cs
// This view model file that carries validated form or dashboard data between controllers and Razor views.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // පින්තූර අප්ලෝඩ් (IFormFile) සඳහා මෙය අත්‍යවශ්‍ය වේ

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.ViewModels;

// ViewModel class: contains only the data needed by a form or page, often with validation rules.
public class ProfileEditViewModel
{
    // --- View එකේ තිබුණු Hidden Field දත්ත සඳහා අත්‍යවශ්‍ය කොටස් ---
    // Primary key identifier for this record in the database.
    public int Id { get; set; }
    // Foreign key/string link to the ASP.NET Identity user account.
    public string UserId { get; set; } = string.Empty;
    // Stores the OCVMS role name, usually Admin, Organizer, or Volunteer.
    public string RoleName { get; set; } = string.Empty;
    // Path or URL of the user profile image.
    public string? ProfileImageUrl { get; set; }

    // --- සාමාන්‍ය ෆෝම් දත්ත (ඉංග්‍රීසි එරර්ස් සමඟ) ---
    // Validation attribute: the form cannot be submitted successfully without this value.
    [Required(ErrorMessage = "Full Name is required.")]
    [Display(Name = "Full Name")]
    // Display name used in profiles, posts, registrations, and admin pages.
    public string FullName { get; set; } = string.Empty;

    // Validation attribute: checks that the value has an email address format.
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Public Email")]
    // Email address shown or stored as part of the OCVMS user profile.
    public string? PublicEmail { get; set; }

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [Display(Name = "Contact Number")]
    // Phone/contact information stored in the user profile.
    public string? ContactNumber { get; set; }

    // Volunteer availability information shown in profile pages.

    public string? Availability { get; set; }
    
    // Volunteer skills used to describe abilities or interests.
    
    public string? Skills { get; set; }
    
    [Display(Name = "Organization Name")]
    // Stores the organization name for organizer profiles and ownership checks.
    public string? OrganizationName { get; set; }
    
    // Short profile description written by the user.
    
    public string? Bio { get; set; }

    // --- අලුත් පින්තූරය අප්ලෝඩ් කිරීමට අවශ්‍ය කොටස ---
    [Display(Name = "Upload New Profile Photo")]
    // Stores the ProfileImage value used by the application, database, or Razor view.
    public IFormFile? ProfileImage { get; set; }
}
