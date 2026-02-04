using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Models.DTOs.Events;

/// <summary>
/// Response DTO containing complete event information.
/// </summary>
public class EventResponse
{
    /// <summary>
    /// Unique identifier for the event.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Event title/name.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the event.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Event location address or venue name.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Event start date and time.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Duration of the event in minutes.
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Maximum number of volunteers that can register.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Optional URL for the event image/banner.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Deadline for volunteer registration.
    /// </summary>
    public DateTime? RegistrationDeadline { get; set; }

    /// <summary>
    /// Current status of the event (Active or Cancelled).
    /// </summary>
    public EventStatus Status { get; set; }

    /// <summary>
    /// Number of confirmed registrations for this event.
    /// </summary>
    public int RegistrationCount { get; set; }

    /// <summary>
    /// Number of available spots remaining (Capacity - RegistrationCount).
    /// </summary>
    public int AvailableSpots { get; set; }

    /// <summary>
    /// Indicates whether the event has reached its capacity limit.
    /// </summary>
    public bool IsFull { get; set; }

    /// <summary>
    /// Foreign key to the user who organized this event.
    /// </summary>
    public int OrganizerId { get; set; }

    /// <summary>
    /// Name of the organizer who created this event.
    /// </summary>
    public string OrganizerName { get; set; } = string.Empty;

    /// <summary>
    /// Email of the organizer who created this event.
    /// </summary>
    public string OrganizerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the event was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// List of skills required for this event.
    /// </summary>
    public List<SkillResponse> RequiredSkills { get; set; } = new();
}
