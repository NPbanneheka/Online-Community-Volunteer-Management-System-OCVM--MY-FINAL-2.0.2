// ViewModels/LoginViewModel.cs
// This view model file that carries validated form or dashboard data between controllers and Razor views.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
using System.ComponentModel.DataAnnotations;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.ViewModels;

// ViewModel class: contains only the data needed by a form or page, often with validation rules.
public class LoginViewModel
{
    // Validation attribute: the form cannot be submitted successfully without this value.
    [Required(ErrorMessage = "ඊමේල් ලිපිනය ඇතුළත් කරන්න.")]
    // Validation attribute: checks that the value has an email address format.
    [EmailAddress(ErrorMessage = "වලංගු ඊමේල් ලිපිනයක් අවශ්‍යයි.")]
    // Email value used for login or account-related forms.
    public string Email { get; set; } = string.Empty;

    // Validation attribute: the form cannot be submitted successfully without this value.
    [Required(ErrorMessage = "මුරපදය ඇතුළත් කරන්න.")]
    [DataType(DataType.Password)]
    // Password value submitted by the user and processed by Identity hashing.
    public string Password { get; set; } = string.Empty;

    [Display(Name = "මාව මතක තබා ගන්න (Remember Me)")]
    // Stores the RememberMe value used by the application, database, or Razor view.
    public bool RememberMe { get; set; }
}
