// ================================================================
// VIVA COMMENTED VERSION - ViewModels/LoginViewModel.cs
// Purpose: ViewModel file: carries validated data between Razor forms/views and controller actions without exposing full database entities.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

using System.ComponentModel.DataAnnotations;

namespace OCVMS.ViewModels;
// This class defines structured data used by the application.
public class LoginViewModel
{
    [Required(ErrorMessage = "ඊමේල් ලිපිනය ඇතුළත් කරන්න.")]
    [EmailAddress(ErrorMessage = "වලංගු ඊමේල් ලිපිනයක් අවශ්‍යයි.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "මුරපදය ඇතුළත් කරන්න.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "මාව මතක තබා ගන්න (Remember Me)")]
    public bool RememberMe { get; set; }
}