namespace VolunteerPortal.API.Models.Enums;

/// <summary>
/// Defines the current status of an event.
/// </summary>
public enum EventStatus
{
    /// <summary>
    /// Event is active and accepting registrations.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Event has been cancelled and is no longer accepting registrations.
    /// </summary>
    Cancelled = 1
}
