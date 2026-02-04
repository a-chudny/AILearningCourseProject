using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunteerPortal.API.Models.DTOs.Events;
using VolunteerPortal.API.Models.Enums;
using VolunteerPortal.API.Services.Interfaces;

namespace VolunteerPortal.API.Controllers;

/// <summary>
/// Controller for managing volunteer events
/// </summary>
[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IFileStorageService _fileStorageService;

    public EventsController(
        IEventService eventService,
        IFileStorageService fileStorageService)
    {
        _eventService = eventService;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Get a paginated list of events
    /// </summary>
    /// <param name="queryParams">Query parameters for filtering, sorting, and pagination</param>
    /// <returns>Paginated list of events</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EventListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EventListResponse>> GetEvents([FromQuery] EventQueryParams queryParams)
    {
        // Extract userId from claims if authenticated (for "Match My Skills" feature)
        int? currentUserId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId))
            {
                currentUserId = userId;
            }
        }

        var result = await _eventService.GetAllAsync(queryParams, currentUserId);
        return Ok(result);
    }

    /// <summary>
    /// Get event details by ID
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <returns>Event details</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> GetEventById(int id)
    {
        var eventResponse = await _eventService.GetByIdAsync(id);
        if (eventResponse == null)
        {
            return NotFound(new { message = $"Event with ID {id} not found." });
        }
        return Ok(eventResponse);
    }

    /// <summary>
    /// Create a new event
    /// </summary>
    /// <param name="request">Event creation request</param>
    /// <returns>Created event details</returns>
    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EventResponse>> CreateEvent([FromBody] CreateEventRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var eventResponse = await _eventService.CreateAsync(request, userId);
            return CreatedAtAction(
                nameof(GetEventById),
                new { id = eventResponse.Id },
                eventResponse
            );
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing event
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <param name="request">Event update request</param>
    /// <returns>Updated event details</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var eventResponse = await _eventService.UpdateAsync(id, request, userId);
            return Ok(eventResponse);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete an event (soft delete)
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            await _eventService.DeleteAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Upload an image for an event
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <param name="file">Image file (JPG or PNG, max 5MB)</param>
    /// <returns>Updated event with image URL</returns>
    [HttpPost("{id}/image")]
    [Authorize(Roles = "Organizer,Admin")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> UploadEventImage(int id, [FromForm] IFormFile file)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Verify event exists and user has permission
            var existingEvent = await _eventService.GetByIdAsync(id);
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            
            if (existingEvent.OrganizerId != userId && userRole != "Admin")
            {
                return StatusCode(StatusCodes.Status403Forbidden, 
                    new { message = "You don't have permission to upload images for this event." });
            }

            // Delete old image if exists
            if (!string.IsNullOrEmpty(existingEvent.ImageUrl))
            {
                await _fileStorageService.DeleteAsync(existingEvent.ImageUrl);
            }

            // Upload new image
            var imageUrl = await _fileStorageService.UploadAsync(file, "events");

            // Update event with new image URL
            var updateRequest = new UpdateEventRequest
            {
                Title = existingEvent.Title,
                Description = existingEvent.Description,
                Location = existingEvent.Location,
                StartTime = existingEvent.StartTime,
                DurationMinutes = existingEvent.DurationMinutes,
                Capacity = existingEvent.Capacity,
                ImageUrl = imageUrl,
                RegistrationDeadline = existingEvent.RegistrationDeadline,
                RequiredSkillIds = existingEvent.RequiredSkills.Select(s => s.Id).ToList(),
                Status = existingEvent.Status
            };

            var updatedEvent = await _eventService.UpdateAsync(id, updateRequest, userId);
            return Ok(updatedEvent);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (IOException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete an event's image
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <returns>Updated event without image</returns>
    [HttpDelete("{id}/image")]
    [Authorize(Roles = "Organizer,Admin")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> DeleteEventImage(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Verify event exists and user has permission
            var existingEvent = await _eventService.GetByIdAsync(id);
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            
            if (existingEvent.OrganizerId != userId && userRole != "Admin")
            {
                return StatusCode(StatusCodes.Status403Forbidden, 
                    new { message = "You don't have permission to delete images for this event." });
            }

            // Delete image file if exists
            if (!string.IsNullOrEmpty(existingEvent.ImageUrl))
            {
                await _fileStorageService.DeleteAsync(existingEvent.ImageUrl);
            }

            // Update event to remove image URL
            var updateRequest = new UpdateEventRequest
            {
                Title = existingEvent.Title,
                Description = existingEvent.Description,
                Location = existingEvent.Location,
                StartTime = existingEvent.StartTime,
                DurationMinutes = existingEvent.DurationMinutes,
                Capacity = existingEvent.Capacity,
                ImageUrl = null,
                RegistrationDeadline = existingEvent.RegistrationDeadline,
                RequiredSkillIds = existingEvent.RequiredSkills.Select(s => s.Id).ToList(),
                Status = existingEvent.Status
            };

            var updatedEvent = await _eventService.UpdateAsync(id, updateRequest, userId);
            return Ok(updatedEvent);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel an event
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <returns>Updated event with cancelled status</returns>
    [HttpPut("{id}/cancel")]
    [Authorize(Roles = "Organizer,Admin")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> CancelEvent(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Verify event exists and user has permission
            var existingEvent = await _eventService.GetByIdAsync(id);
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            
            if (existingEvent.OrganizerId != userId && userRole != "Admin")
            {
                return StatusCode(StatusCodes.Status403Forbidden, 
                    new { message = "You don't have permission to cancel this event." });
            }

            // Validate event is Active
            if (existingEvent.Status != EventStatus.Active)
            {
                return BadRequest(new { message = "Only active events can be cancelled." });
            }

            // Update event to Cancelled status
            var updateRequest = new UpdateEventRequest
            {
                Title = existingEvent.Title,
                Description = existingEvent.Description,
                Location = existingEvent.Location,
                StartTime = existingEvent.StartTime,
                DurationMinutes = existingEvent.DurationMinutes,
                Capacity = existingEvent.Capacity,
                ImageUrl = existingEvent.ImageUrl,
                RegistrationDeadline = existingEvent.RegistrationDeadline,
                RequiredSkillIds = existingEvent.RequiredSkills.Select(s => s.Id).ToList(),
                Status = EventStatus.Cancelled
            };

            var updatedEvent = await _eventService.UpdateAsync(id, updateRequest, userId);
            return Ok(updatedEvent);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
