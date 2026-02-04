using VolunteerPortal.API.Models.DTOs.Registrations;

namespace VolunteerPortal.API.Services.Interfaces;

/// <summary>
/// Service interface for event registration operations
/// </summary>
public interface IRegistrationService
{
    /// <summary>
    /// Register a user for an event
    /// </summary>
    Task<RegistrationResponse> RegisterForEventAsync(int eventId, int userId);

    /// <summary>
    /// Cancel a user's registration for an event
    /// </summary>
    Task CancelRegistrationAsync(int eventId, int userId);

    /// <summary>
    /// Get all registrations for a specific user
    /// </summary>
    Task<IEnumerable<RegistrationResponse>> GetUserRegistrationsAsync(int userId);

    /// <summary>
    /// Get all registrations for a specific event
    /// </summary>
    Task<IEnumerable<EventRegistrationResponse>> GetEventRegistrationsAsync(int eventId);
}
