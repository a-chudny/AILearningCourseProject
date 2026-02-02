namespace VolunteerPortal.API.Models.Enums;

/// <summary>
/// Defines the status of a volunteer's event registration.
/// </summary>
public enum RegistrationStatus
{
    /// <summary>
    /// Registration is confirmed and active.
    /// </summary>
    Confirmed = 0,

    /// <summary>
    /// Registration has been cancelled by the volunteer or organizer.
    /// </summary>
    Cancelled = 1
}
