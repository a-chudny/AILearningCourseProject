namespace VolunteerPortal.API.Models.DTOs.Admin;

/// <summary>
/// Response DTO for admin dashboard statistics
/// </summary>
public class AdminStatsResponse
{
    /// <summary>
    /// Total number of users (all roles, excluding soft-deleted)
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Total number of events (excluding soft-deleted)
    /// </summary>
    public int TotalEvents { get; set; }

    /// <summary>
    /// Total number of registrations (all statuses)
    /// </summary>
    public int TotalRegistrations { get; set; }

    /// <summary>
    /// Number of registrations created in the current calendar month
    /// </summary>
    public int RegistrationsThisMonth { get; set; }

    /// <summary>
    /// Number of upcoming events (future events that are active)
    /// </summary>
    public int UpcomingEvents { get; set; }
}
