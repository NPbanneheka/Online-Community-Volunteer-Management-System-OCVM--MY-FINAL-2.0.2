using System.ComponentModel.DataAnnotations;

namespace OCVMS.ViewModels;

public class ProfileEditViewModel
{
    [Required]
    public string FullName { get; set; } = string.Empty;
    public string? PublicEmail { get; set; }
    public string? ContactNumber { get; set; }
    public string? Bio { get; set; }
    public string? Skills { get; set; }
    public string? Availability { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? OrganizationName { get; set; }
}
