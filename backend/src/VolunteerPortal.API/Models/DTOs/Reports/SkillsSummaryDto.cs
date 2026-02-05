namespace VolunteerPortal.API.Models.DTOs.Reports;

/// <summary>
/// DTO for exporting skills summary to Excel
/// </summary>
public class SkillsSummaryDto
{
    public int Id { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int VolunteerCount { get; set; }
    public int EventCount { get; set; }
}
