using System.ComponentModel.DataAnnotations;

namespace VolunteerPortal.API.Models.DTOs.Events;

/// <summary>
/// Request DTO for creating a new event.
/// </summary>
public class CreateEventRequest
{
    /// <summary>
    /// Event title/name (required, max 200 characters).
    /// </summary>
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the event (required, max 2000 characters).
    /// </summary>
    [Required(ErrorMessage = "Description is required.")]
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Event location address or venue name (required, max 500 characters).
    /// </summary>
    [Required(ErrorMessage = "Location is required.")]
    [MaxLength(500, ErrorMessage = "Location cannot exceed 500 characters.")]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Event start date and time (required, must be in the future).
    /// </summary>
    [Required(ErrorMessage = "Start time is required.")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Duration of the event in minutes (required, must be positive).
    /// </summary>
    [Required(ErrorMessage = "Duration is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 minute.")]
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Maximum number of volunteers that can register (required, must be positive).
    /// </summary>
    [Required(ErrorMessage = "Capacity is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1.")]
    public int Capacity { get; set; }

    /// <summary>
    /// Optional URL for the event image/banner (max 500 characters).
    /// </summary>
    [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Optional deadline for volunteer registration (must be before event start time).
    /// </summary>
    public DateTime? RegistrationDeadline { get; set; }

    /// <summary>
    /// List of skill IDs required for this event (optional).
    /// </summary>
    public List<int> RequiredSkillIds { get; set; } = new();
}
