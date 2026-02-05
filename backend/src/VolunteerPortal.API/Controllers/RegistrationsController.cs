using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunteerPortal.API.Application.Registrations.Commands;
using VolunteerPortal.API.Application.Registrations.Queries;
using VolunteerPortal.API.Models.DTOs.Registrations;

namespace VolunteerPortal.API.Controllers;

/// <summary>
/// API endpoints for event registration management.
/// Provides endpoints for users to register for events, cancel registrations, and view their registrations.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
public class RegistrationsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationsController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR mediator for dispatching commands and queries.</param>
    public RegistrationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register current user for an event.
    /// </summary>
    /// <remarks>
    /// Creates a new registration for the authenticated user.
    /// Users cannot register for the same event twice.
    /// Registration may fail if the event is at capacity or past the registration deadline.
    /// </remarks>
    /// <param name="id">The unique identifier of the event to register for.</param>
    /// <returns>The created registration details.</returns>
    /// <response code="201">Registration created successfully.</response>
    /// <response code="400">Invalid request or validation error.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Event not found.</response>
    /// <response code="409">User is already registered for this event or event is at capacity.</response>
    [HttpPost("events/{id}/register")]
    [Authorize]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegistrationResponse>> RegisterForEvent(int id)
    {
        var userId = GetCurrentUserId();
        var command = new RegisterForEventCommand(id, userId);
        var registration = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetUserRegistrations), new { }, registration);
    }

    /// <summary>
    /// Cancel current user's registration for an event.
    /// </summary>
    /// <remarks>
    /// Cancels an existing registration for the authenticated user.
    /// Cancelled registrations can be recreated by registering again.
    /// </remarks>
    /// <param name="id">The unique identifier of the event to cancel registration for.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Registration cancelled successfully.</response>
    /// <response code="400">Cannot cancel this registration (e.g., event already started).</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Registration not found.</response>
    [HttpDelete("events/{id}/register")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelRegistration(int id)
    {
        var userId = GetCurrentUserId();
        var command = new CancelRegistrationCommand(id, userId);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Get all registrations for current user.
    /// </summary>
    /// <remarks>
    /// Returns all registrations (confirmed, pending, and cancelled) for the authenticated user,
    /// including event details like title, start time, and location.
    /// </remarks>
    /// <returns>List of user's registrations with event details.</returns>
    /// <response code="200">Returns the list of registrations.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("users/me/registrations")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<RegistrationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<RegistrationResponse>>> GetUserRegistrations()
    {
        var userId = GetCurrentUserId();
        var query = new GetUserRegistrationsQuery(userId);
        var registrations = await _mediator.Send(query);
        return Ok(registrations);
    }

    /// <summary>
    /// Get all registrations for a specific event.
    /// </summary>
    /// <remarks>
    /// Returns all registrations for an event, including user details.
    /// Only event organizers and admins can view event registrations.
    /// </remarks>
    /// <param name="id">The unique identifier of the event.</param>
    /// <returns>List of event registrations with user details.</returns>
    /// <response code="200">Returns the list of event registrations.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have permission to view event registrations.</response>
    [HttpGet("events/{id}/registrations")]
    [Authorize(Roles = "Organizer,Admin")]
    [ProducesResponseType(typeof(IEnumerable<EventRegistrationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<EventRegistrationResponse>>> GetEventRegistrations(int id)
    {
        var query = new GetEventRegistrationsQuery(id);
        var registrations = await _mediator.Send(query);
        return Ok(registrations);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token");
        return userId;
    }
}
