<<<<<<< HEAD
// ViewModels/RegisterViewModel.cs
// This view model file that carries validated form or dashboard data between controllers and Razor views.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
=======
// View model for RegisterViewModel.
// Technology map:
// - ASP.NET Core MVC uses this class to transfer form/page data between Controller and Razor View.
// - DataAnnotation attributes provide validation rules shown in the UI.
// Connected files: Controllers receive this model; Views bind form fields to these properties.

>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
using System.ComponentModel.DataAnnotations;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.ViewModels;
<<<<<<< HEAD

// ViewModel class: contains only the data needed by a form or page, often with validation rules.
=======
// This class defines structured data used by the application.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
public class RegisterViewModel
{
    // Validation attribute: the form cannot be submitted successfully without this value.
    [Required(ErrorMessage = "Full Name is required.")]
    [Display(Name = "Full Name")]
<<<<<<< HEAD
    // Display name used in profiles, posts, registrations, and admin pages.
=======
    // Readable user name shown in profile and admin screens.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
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
<<<<<<< HEAD
    // Stores the organization name for organizer profiles and ownership checks.
=======
    // Organizer organization name used for ownership and grouping.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
    public string? OrganizationName { get; set; }
}
