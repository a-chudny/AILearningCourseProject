using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerPortal.API.Services.Interfaces;

namespace VolunteerPortal.API.Controllers;

/// <summary>
/// Controller for admin report exports
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IExportService _exportService;
    private readonly ILogger<ReportsController> _logger;

    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public ReportsController(
        IReportService reportService,
        IExportService exportService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _exportService = exportService;
        _logger = logger;
    }

    /// <summary>
    /// Export all users to Excel
    /// </summary>
    /// <returns>Excel file with user data</returns>
    [HttpGet("users/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportUsers()
    {
        _logger.LogInformation("Admin requested users export");

        var users = await _reportService.GetUsersForExportAsync();
        var excelBytes = await _exportService.ExportToExcelAsync(users, "Users");
        var contentDisposition = _exportService.GetContentDisposition("users");

        Response.Headers.ContentDisposition = contentDisposition;
        return File(excelBytes, ExcelContentType);
    }

    /// <summary>
    /// Export all events to Excel
    /// </summary>
    /// <returns>Excel file with event data</returns>
    [HttpGet("events/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportEvents()
    {
        _logger.LogInformation("Admin requested events export");

        var events = await _reportService.GetEventsForExportAsync();
        var excelBytes = await _exportService.ExportToExcelAsync(events, "Events");
        var contentDisposition = _exportService.GetContentDisposition("events");

        Response.Headers.ContentDisposition = contentDisposition;
        return File(excelBytes, ExcelContentType);
    }

    /// <summary>
    /// Export registrations to Excel with optional date filters
    /// </summary>
    /// <param name="startDate">Optional start date filter (filters by registration date)</param>
    /// <param name="endDate">Optional end date filter (filters by registration date)</param>
    /// <returns>Excel file with registration data</returns>
    [HttpGet("registrations/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportRegistrations(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        _logger.LogInformation("Admin requested registrations export with filters: startDate={StartDate}, endDate={EndDate}", 
            startDate, endDate);

        var registrations = await _reportService.GetRegistrationsForExportAsync(startDate, endDate);
        var excelBytes = await _exportService.ExportToExcelAsync(registrations, "Registrations");
        var contentDisposition = _exportService.GetContentDisposition("registrations");

        Response.Headers.ContentDisposition = contentDisposition;
        return File(excelBytes, ExcelContentType);
    }

    /// <summary>
    /// Export skills summary to Excel
    /// </summary>
    /// <returns>Excel file with skills summary</returns>
    [HttpGet("skills/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportSkillsSummary()
    {
        _logger.LogInformation("Admin requested skills summary export");

        var skillsSummary = await _reportService.GetSkillsSummaryForExportAsync();
        var excelBytes = await _exportService.ExportToExcelAsync(skillsSummary, "Skills Summary");
        var contentDisposition = _exportService.GetContentDisposition("skills_summary");

        Response.Headers.ContentDisposition = contentDisposition;
        return File(excelBytes, ExcelContentType);
    }
}
