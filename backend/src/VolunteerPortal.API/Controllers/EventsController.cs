using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunteerPortal.API.Application.Events.Commands;
using VolunteerPortal.API.Application.Events.Queries;
using VolunteerPortal.API.Models.DTOs.Events;

namespace VolunteerPortal.API.Controllers;

/// <summary>
/// Controller for managing volunteer events.
/// Provides endpoints for CRUD operations, image management, and event status changes.
/// </summary>
[ApiController]
[Route("api/events")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventsController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR mediator for dispatching commands and queries.</param>
    public EventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a paginated list of events.
    /// </summary>
    /// <remarks>
    /// Supports filtering by date range, location, status, and required skills.
    /// Authenticated users can filter events matching their skills.
    /// Results are paginated with configurable page size.
    /// </remarks>
    /// <param name="queryParams">Query parameters for filtering, sorting, and pagination.</param>
    /// <returns>Paginated list of events with metadata.</returns>
    /// <response code="200">Returns the paginated event list.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EventListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EventListResponse>> GetEvents([FromQuery] EventQueryParams queryParams)
    {
        int? currentUserId = null;
        if (User.Identity?.IsAuthenticated == true && int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
            currentUserId = uid;

        var query = new GetEventsQuery(queryParams, currentUserId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get event details by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the event.</param>
    /// <returns>Event details including organizer info and required skills.</returns>
    /// <response code="200">Returns the event details.</response>
    /// <response code="404">Event not found.</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> GetEventById(int id)
    {
        var query = new GetEventByIdQuery(id);
        var eventResponse = await _mediator.Send(query);
        return eventResponse == null 
            ? NotFound(new { message = $"Event with ID {id} not found." }) 
            : Ok(eventResponse);
    }

    /// <summary>
    /// Create a new event.
    /// </summary>
    /// <remarks>
    /// Requires Organizer or Admin role. The authenticated user becomes the event organizer.
    /// Event start time must be in the future. Registration deadline (if provided) must be before start time.
    /// </remarks>
    /// <param name="request">Event creation details.</param>
    /// <returns>The created event details.</returns>
    /// <response code="201">Event created successfully.</response>
    /// <response code="400">Invalid request data or validation error.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have required role.</response>
    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EventResponse>> CreateEvent([FromBody] CreateEventRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new CreateEventCommand(request, userId);
        var eventResponse = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetEventById), new { id = eventResponse.Id }, eventResponse);
    }

    /// <summary>
    /// Update an existing event.
    /// </summary>
    /// <remarks>
    /// Only the event organizer or an Admin can update the event.
    /// </remarks>
    /// <param name="id">The unique identifier of the event to update.</param>
    /// <param name="request">Updated event details.</param>
    /// <returns>The updated event details.</returns>
    /// <response code="200">Event updated successfully.</response>
    /// <response code="400">Invalid request data or validation error.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have permission to update this event.</response>
    /// <response code="404">Event not found.</response>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateEventCommand(id, request, userId);
        var eventResponse = await _mediator.Send(command);
        return Ok(eventResponse);
    }

    /// <summary>
    /// Delete an event (soft delete).
    /// </summary>
    /// <remarks>
    /// Only the event organizer or an Admin can delete the event.
    /// This performs a soft delete - the event is marked as deleted but not removed from the database.
    /// </remarks>
    /// <param name="id">The unique identifier of the event to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Event deleted successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have permission to delete this event.</response>
    /// <response code="404">Event not found.</response>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var userId = GetCurrentUserId();
        var command = new DeleteEventCommand(id, userId);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Upload an image for an event.
    /// </summary>
    /// <remarks>
    /// Accepts JPG or PNG images up to 5MB. Only the event organizer or an Admin can upload images.
    /// If the event already has an image, the old image is deleted and replaced.
    /// </remarks>
    /// <param name="id">The unique identifier of the event.</param>
    /// <param name="file">The image file to upload (JPG or PNG, max 5MB).</param>
    /// <returns>Updated event with the new image URL.</returns>
    /// <response code="200">Image uploaded successfully.</response>
    /// <response code="400">Invalid file format or file too large.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have permission to upload images for this event.</response>
    /// <response code="404">Event not found.</response>
    [HttpPost("{id}/image")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Organizer,Admin")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> UploadEventImage(int id, IFormFile file)
    {
        var userId = GetCurrentUserId();
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var command = new UploadEventImageCommand(id, file, userId, userRole);
        var updatedEvent = await _mediator.Send(command);
        return Ok(updatedEvent);
    }

    /// <summary>
    /// Delete an event's image.
    /// </summary>
    /// <remarks>
    /// Only the event organizer or an Admin can delete the event image.
    /// </remarks>
    /// <param name="id">The unique identifier of the event.</param>
    /// <returns>Updated event without the image.</returns>
    /// <response code="200">Image deleted successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have permission to delete images for this event.</response>
    /// <response code="404">Event not found.</response>
    [HttpDelete("{id}/image")]
    [Authorize(Roles = "Organizer,Admin")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> DeleteEventImage(int id)
    {
        var userId = GetCurrentUserId();
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var command = new DeleteEventImageCommand(id, userId, userRole);
        var updatedEvent = await _mediator.Send(command);
        return Ok(updatedEvent);
    }

    /// <summary>
    /// Cancel an event.
    /// </summary>
    /// <remarks>
    /// Only the event organizer or an Admin can cancel the event.
    /// Only active events can be cancelled.
    /// </remarks>
    /// <param name="id">The unique identifier of the event to cancel.</param>
    /// <returns>Updated event with cancelled status.</returns>
    /// <response code="200">Event cancelled successfully.</response>
    /// <response code="400">Event is not in active status.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have permission to cancel this event.</response>
    /// <response code="404">Event not found.</response>
    [HttpPut("{id}/cancel")]
    [Authorize(Roles = "Organizer,Admin")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> CancelEvent(int id)
    {
        var userId = GetCurrentUserId();
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var command = new CancelEventCommand(id, userId, userRole);
        var updatedEvent = await _mediator.Send(command);
        return Ok(updatedEvent);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("Invalid token");
        return userId;
    }
}
