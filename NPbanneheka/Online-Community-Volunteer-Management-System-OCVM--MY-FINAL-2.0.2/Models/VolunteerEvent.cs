using System.ComponentModel.DataAnnotations;

namespace OCVMS.Models;

public class VolunteerEvent : BaseEntity
{
    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string Location { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    [Range(1, 10000)]
    public int Capacity { get; set; }

    public string Status { get; set; } = "Upcoming";

    public string OrganizerId { get; set; } = string.Empty;

    [StringLength(250)]
    public string? ImageUrl { get; set; }
}
