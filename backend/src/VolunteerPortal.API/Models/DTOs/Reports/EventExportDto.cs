namespace VolunteerPortal.API.Models.DTOs.Reports;

/// <summary>
/// DTO for exporting event data to Excel
/// </summary>
public class EventExportDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int RegistrationCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;
    public string OrganizerEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
