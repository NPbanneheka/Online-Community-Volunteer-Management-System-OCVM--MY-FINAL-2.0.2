namespace OCVMS.Models;

public class EventRegistration : BaseEntity
{
    public int VolunteerEventId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AttendanceStatus { get; set; } = "Pending";
}
