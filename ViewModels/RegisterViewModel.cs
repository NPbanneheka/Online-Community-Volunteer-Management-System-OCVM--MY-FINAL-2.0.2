// View model for RegisterViewModel.
// Technology map:
// - ASP.NET Core MVC uses this class to transfer form/page data between Controller and Razor View.
// - DataAnnotation attributes provide validation rules shown in the UI.
// Connected files: Controllers receive this model; Views bind form fields to these properties.

using System.ComponentModel.DataAnnotations;

namespace OCVMS.ViewModels;
// This class defines structured data used by the application.
public class RegisterViewModel
{
    [Required(ErrorMessage = "Full Name is required.")]
    [Display(Name = "Full Name")]
    // Readable user name shown in profile and admin screens.
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a role.")]
    [Display(Name = "Register As")]
    public string RoleName { get; set; } = "Volunteer";

    [Display(Name = "Organization Name")]
    [StringLength(120, ErrorMessage = "Organization name cannot be longer than 120 characters.")]
    // Organizer organization name used for ownership and grouping.
    public string? OrganizationName { get; set; }
}
