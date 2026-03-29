using System.ComponentModel.DataAnnotations;

namespace OCVMS.Models;

public class UserProfile : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    public string? PublicEmail { get; set; }

    [StringLength(20)]
    public string? ContactNumber { get; set; }

    [StringLength(250)]
    public string? Bio { get; set; }

    [StringLength(250)]
    public string? Skills { get; set; }

    [StringLength(120)]
    public string? Availability { get; set; }

    [StringLength(250)]
    public string? ProfileImageUrl { get; set; }

    [StringLength(120)]
    public string? OrganizationName { get; set; }

    public bool IsVerified { get; set; }

    public string RoleName { get; set; } = "Volunteer";
}
