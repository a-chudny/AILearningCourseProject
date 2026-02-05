using VolunteerPortal.API.Models.DTOs.Reports;

namespace VolunteerPortal.API.Services.Interfaces;

/// <summary>
/// Service interface for generating export reports
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Get all users for export (active only)
    /// </summary>
    Task<IEnumerable<UserExportDto>> GetUsersForExportAsync();

    /// <summary>
    /// Get all events for export (active only)
    /// </summary>
    Task<IEnumerable<EventExportDto>> GetEventsForExportAsync();

    /// <summary>
    /// Get all registrations for export with optional date filter
    /// </summary>
    /// <param name="startDate">Optional start date filter (based on RegisteredAt)</param>
    /// <param name="endDate">Optional end date filter (based on RegisteredAt)</param>
    Task<IEnumerable<RegistrationExportDto>> GetRegistrationsForExportAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get skills summary for export
    /// </summary>
    Task<IEnumerable<SkillsSummaryDto>> GetSkillsSummaryForExportAsync();
}
