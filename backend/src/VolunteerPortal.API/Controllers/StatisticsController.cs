using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Controllers;

/// <summary>
/// Controller for platform statistics
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StatisticsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get platform statistics
    /// </summary>
    /// <returns>Platform statistics</returns>
    [HttpGet]
    [ProducesResponseType(typeof(StatisticsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StatisticsResponse>> GetStatistics()
    {
        var totalEvents = await _context.Events
            .Where(e => e.Status == EventStatus.Active && !e.IsDeleted)
            .CountAsync();

        var totalVolunteers = await _context.Users
            .Where(u => u.Role == UserRole.Volunteer && !u.IsDeleted)
            .CountAsync();

        var totalRegistrations = await _context.Registrations
            .Where(r => r.Status == RegistrationStatus.Confirmed)
            .CountAsync();

        return Ok(new StatisticsResponse
        {
            TotalEvents = totalEvents,
            TotalVolunteers = totalVolunteers,
            TotalRegistrations = totalRegistrations
        });
    }
}

/// <summary>
/// Response DTO for platform statistics
/// </summary>
public class StatisticsResponse
{
    /// <summary>
    /// Total number of active events
    /// </summary>
    public int TotalEvents { get; set; }

    /// <summary>
    /// Total number of registered volunteers
    /// </summary>
    public int TotalVolunteers { get; set; }

    /// <summary>
    /// Total number of confirmed registrations
    /// </summary>
    public int TotalRegistrations { get; set; }
}
