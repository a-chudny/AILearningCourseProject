using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Models.Entities;

/// <summary>
/// Represents a volunteer event in the system.
/// </summary>
public class Event
{
    /// <summary>
    /// Unique identifier for the event.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Event title/name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the event.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Event location address or venue name.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Event start date and time.
    /// </summary>
    [Required]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Duration of the event in minutes.
    /// </summary>
    [Required]
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Maximum number of volunteers that can register.
    /// </summary>
    [Required]
    public int Capacity { get; set; }

    /// <summary>
    /// Optional URL for the event image/banner.
    /// </summary>
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Deadline for volunteer registration.
    /// </summary>
    public DateTime? RegistrationDeadline { get; set; }

    /// <summary>
    /// Current status of the event (Active or Cancelled).
    /// </summary>
    [Required]
    public EventStatus Status { get; set; } = EventStatus.Active;

    /// <summary>
    /// Foreign key to the user who organized this event.
    /// </summary>
    [Required]
    public int OrganizerId { get; set; }

    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the event was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete flag. When true, the event is marked as deleted.
    /// </summary>
    [Required]
    public bool IsDeleted { get; set; } = false;

    // Navigation properties

    /// <summary>
    /// The organizer who created this event.
    /// </summary>
    [ForeignKey(nameof(OrganizerId))]
    public User Organizer { get; set; } = null!;

    /// <summary>
    /// Volunteer registrations for this event.
    /// </summary>
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    /// <summary>
    /// Skills required/associated with this event through join table.
    /// </summary>
    public ICollection<EventSkill> EventSkills { get; set; } = new List<EventSkill>();
}
