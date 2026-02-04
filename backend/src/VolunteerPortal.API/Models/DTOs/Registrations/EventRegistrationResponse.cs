using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Models.DTOs.Registrations;

/// <summary>
/// Response DTO for event's registration list (organizer view)
/// </summary>
public class EventRegistrationResponse
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public RegistrationStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }

    // User summary for organizer's registration list
    public UserSummary User { get; set; } = null!;
}

/// <summary>
/// User summary included in event registration response
/// </summary>
public class UserSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
