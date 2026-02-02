namespace VolunteerPortal.API.Models.Enums;

/// <summary>
/// Defines the roles a user can have in the system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Regular volunteer user who can register for events.
    /// </summary>
    Volunteer = 0,

    /// <summary>
    /// Organizer who can create and manage their own events.
    /// </summary>
    Organizer = 1,

    /// <summary>
    /// Administrator with full system access.
    /// </summary>
    Admin = 2
}
