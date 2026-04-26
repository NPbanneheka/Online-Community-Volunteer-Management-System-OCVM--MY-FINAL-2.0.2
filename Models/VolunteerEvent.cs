using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OCVMS.Models;

public class VolunteerEvent : BaseEntity
{
    [Required(ErrorMessage = "Event Title is required.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event Description is required.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location is required.")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event Date is required.")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime EventDate { get; set; }

    // අලුතින් එකතු කළ 'වෙලාව' සඳහා කොටස
    [Required(ErrorMessage = "Event Time is required.")]
    [DataType(DataType.Time)]
    public TimeSpan EventTime { get; set; }

    [Required(ErrorMessage = "Capacity is required.")]
    [Range(1, 10000, ErrorMessage = "Capacity must be at least 1.")]
    public int Capacity { get; set; }

    public string Status { get; set; } = "Upcoming";
    public string? ImageUrl { get; set; }

    public int OrganizerProfileId { get; set; }

    [ForeignKey("OrganizerProfileId")]
    [ValidateNever] 
    public virtual UserProfile? OrganizerProfile { get; set; }

    [ValidateNever]
    public virtual ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
}