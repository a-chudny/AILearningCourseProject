using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunteerPortal.API.Models.DTOs.Registrations;
using VolunteerPortal.API.Models.Enums;
using VolunteerPortal.API.Services.Interfaces;

namespace VolunteerPortal.API.Controllers;

/// <summary>
/// API endpoints for event registration management
/// </summary>
[ApiController]
[Route("api")]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationsController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    /// <summary>
    /// Register current user for an event
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <returns>Registration details</returns>
    [HttpPost("events/{id}/register")]
    [Authorize]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegistrationResponse>> RegisterForEvent(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var registration = await _registrationService.RegisterForEventAsync(id, userId);
            return CreatedAtAction(nameof(GetUserRegistrations), new { }, registration);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel current user's registration for an event
    /// </summary>
    /// <param name="id">Event ID</param>
    [HttpDelete("events/{id}/register")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelRegistration(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _registrationService.CancelRegistrationAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all registrations for current user
    /// </summary>
    /// <returns>List of user's registrations</returns>
    [HttpGet("users/me/registrations")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<RegistrationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RegistrationResponse>>> GetUserRegistrations()
    {
        var userId = GetCurrentUserId();
        var registrations = await _registrationService.GetUserRegistrationsAsync(userId);
        return Ok(registrations);
    }

    /// <summary>
    /// Get all registrations for a specific event (Organizer/Admin only)
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <returns>List of event registrations</returns>
    [HttpGet("events/{id}/registrations")]
    [Authorize(Roles = "Organizer,Admin")]
    [ProducesResponseType(typeof(IEnumerable<EventRegistrationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventRegistrationResponse>>> GetEventRegistrations(int id)
    {
        var registrations = await _registrationService.GetEventRegistrationsAsync(id);
        return Ok(registrations);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}
