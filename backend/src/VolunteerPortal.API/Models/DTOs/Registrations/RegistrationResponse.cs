using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Models.DTOs.Registrations;

/// <summary>
/// Response DTO for user's registration information
/// </summary>
public class RegistrationResponse
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public RegistrationStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }

    // Event summary for user's registration list
    public EventSummary Event { get; set; } = null!;
}

/// <summary>
/// Event summary included in registration response
/// </summary>
public class EventSummary
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public EventStatus Status { get; set; }
    public string? ImageUrl { get; set; }
}
