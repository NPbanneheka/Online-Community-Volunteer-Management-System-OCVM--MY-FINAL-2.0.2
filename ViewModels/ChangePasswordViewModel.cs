// ViewModels/ChangePasswordViewModel.cs
// This view model file that carries validated form or dashboard data between controllers and Razor views.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
using System.ComponentModel.DataAnnotations;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.ViewModels;

// ViewModel class: contains only the data needed by a form or page, often with validation rules.
public class ChangePasswordViewModel
{
    // Validation attribute: the form cannot be submitted successfully without this value.
    [Required(ErrorMessage = "Current password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    // Existing password required before Identity allows a password change.
    public string CurrentPassword { get; set; } = string.Empty;

    // Validation attribute: the form cannot be submitted successfully without this value.
    [Required(ErrorMessage = "New password is required.")]
    // Validation attribute: limits the allowed length of the submitted text.
    [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    // New password submitted to Identity for secure password replacement.
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    // Validation attribute: compares this value with another property, commonly for password confirmation.
    [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
    // Confirmation value used to validate that the password was typed correctly.
    public string ConfirmPassword { get; set; } = string.Empty;
}
