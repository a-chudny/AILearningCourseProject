namespace VolunteerPortal.API.Models.DTOs.Reports;

/// <summary>
/// DTO for exporting registration data to Excel
/// </summary>
public class RegistrationExportDto
{
    public int Id { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string VolunteerName { get; set; } = string.Empty;
    public string VolunteerEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
