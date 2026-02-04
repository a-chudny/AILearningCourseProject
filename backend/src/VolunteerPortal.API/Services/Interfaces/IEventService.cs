using VolunteerPortal.API.Models.DTOs.Events;

namespace VolunteerPortal.API.Services.Interfaces;

/// <summary>
/// Service interface for managing events.
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Creates a new event with the specified organizer.
    /// </summary>
    /// <param name="request">Event creation data.</param>
    /// <param name="organizerId">ID of the user creating the event.</param>
    /// <returns>Created event information.</returns>
    /// <exception cref="ArgumentException">Thrown when organizer doesn't exist or validation fails.</exception>
    Task<EventResponse> CreateAsync(CreateEventRequest request, int organizerId);

    /// <summary>
    /// Updates an existing event. User must be the owner or an admin.
    /// </summary>
    /// <param name="id">Event ID to update.</param>
    /// <param name="request">Updated event data.</param>
    /// <param name="userId">ID of the user performing the update.</param>
    /// <returns>Updated event information.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when event doesn't exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when user is not the owner.</exception>
    Task<EventResponse> UpdateAsync(int id, UpdateEventRequest request, int userId);

    /// <summary>
    /// Soft deletes an event. User must be the owner or an admin.
    /// </summary>
    /// <param name="id">Event ID to delete.</param>
    /// <param name="userId">ID of the user performing the deletion.</param>
    /// <exception cref="KeyNotFoundException">Thrown when event doesn't exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when user is not the owner.</exception>
    Task DeleteAsync(int id, int userId);

    /// <summary>
    /// Gets a single event by ID, including organizer info and registration count.
    /// </summary>
    /// <param name="id">Event ID to retrieve.</param>
    /// <returns>Event information, or null if not found or soft-deleted.</returns>
    Task<EventResponse?> GetByIdAsync(int id);

    /// <summary>
    /// Gets a paginated list of events based on query parameters.
    /// </summary>
    /// <param name="queryParams">Query parameters for filtering, sorting, and pagination.</param>
    /// <returns>Paginated list of events.</returns>
    Task<EventListResponse> GetAllAsync(EventQueryParams queryParams);
}
