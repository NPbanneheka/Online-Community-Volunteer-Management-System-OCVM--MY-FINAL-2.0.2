// ViewModels/RegisterViewModel.cs
// This view model file that carries validated form or dashboard data between controllers and Razor views.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
using System.ComponentModel.DataAnnotations;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.ViewModels;

// ViewModel class: contains only the data needed by a form or page, often with validation rules.
public class RegisterViewModel
{
    // Validation attribute: the form cannot be submitted successfully without this value.
    [Required(ErrorMessage = "Full Name is required.")]
    [Display(Name = "Full Name")]
    // Display name used in profiles, posts, registrations, and admin pages.
    public string FullName { get; set; } = string.Empty;

    // Validation attribute: the form cannot be submitted successfully without this value.
    [Required(ErrorMessage = "Email Address is required.")]
    // Validation attribute: checks that the value has an email address format.
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    // Email value used for login or account-related forms.
    public string Email { get; set; } = string.Empty;

    // Validation attribute: the form cannot be submitted successfully without this value.
    [Required(ErrorMessage = "Password is required.")]
    // Validation attribute: limits the allowed length of the submitted text.
    [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    // Password value submitted by the user and processed by Identity hashing.
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    // Validation attribute: compares this value with another property, commonly for password confirmation.
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    // Confirmation value used to validate that the password was typed correctly.
    public string ConfirmPassword { get; set; } = string.Empty;

    // Validation attribute: the form cannot be submitted successfully without this value.
    [Required(ErrorMessage = "Please select a role.")]
    [Display(Name = "Register As")]
    // Stores the OCVMS role name, usually Admin, Organizer, or Volunteer.
    public string RoleName { get; set; } = "Volunteer";

    [Display(Name = "Organization Name")]
    // Validation attribute: limits the allowed length of the submitted text.
    [StringLength(120, ErrorMessage = "Organization name cannot be longer than 120 characters.")]
    // Stores the organization name for organizer profiles and ownership checks.
    public string? OrganizationName { get; set; }
}
