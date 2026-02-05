using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.DTOs.Admin;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Controllers;

/// <summary>
/// Controller for admin operations and statistics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ApplicationDbContext context,
        ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get admin dashboard statistics
    /// </summary>
    /// <returns>Admin statistics including user counts, event counts, and registrations</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AdminStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminStatsResponse>> GetStats()
    {
        try
        {
            // Get first and last day of current month
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Total users (excluding soft-deleted)
            var totalUsers = await _context.Users
                .Where(u => !u.IsDeleted)
                .CountAsync();

            // Total events (excluding soft-deleted)
            var totalEvents = await _context.Events
                .Where(e => !e.IsDeleted)
                .CountAsync();

            // Total registrations (all statuses)
            var totalRegistrations = await _context.Registrations
                .CountAsync();

            // Registrations this month
            var registrationsThisMonth = await _context.Registrations
                .Where(r => r.RegisteredAt >= firstDayOfMonth && r.RegisteredAt <= lastDayOfMonth.AddDays(1))
                .CountAsync();

            // Upcoming events (future, active, not deleted)
            var upcomingEvents = await _context.Events
                .Where(e => e.StartTime > now && e.Status == EventStatus.Active && !e.IsDeleted)
                .CountAsync();

            return Ok(new AdminStatsResponse
            {
                TotalUsers = totalUsers,
                TotalEvents = totalEvents,
                TotalRegistrations = totalRegistrations,
                RegistrationsThisMonth = registrationsThisMonth,
                UpcomingEvents = upcomingEvents
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin statistics");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving statistics" });
        }
    }
}
