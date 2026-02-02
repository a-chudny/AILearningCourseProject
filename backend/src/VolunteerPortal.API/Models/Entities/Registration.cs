using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Models.Entities;

/// <summary>
/// Represents a volunteer's registration for an event.
/// </summary>
public class Registration
{
    /// <summary>
    /// Unique identifier for the registration.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the event being registered for.
    /// </summary>
    [Required]
    public int EventId { get; set; }

    /// <summary>
    /// Foreign key to the volunteer (user) registering.
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Current status of the registration (Confirmed or Cancelled).
    /// </summary>
    [Required]
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Confirmed;

    /// <summary>
    /// Timestamp when the registration was created.
    /// </summary>
    [Required]
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional notes from the volunteer about their registration.
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Navigation properties

    /// <summary>
    /// The event this registration is for.
    /// </summary>
    [ForeignKey(nameof(EventId))]
    public Event Event { get; set; } = null!;

    /// <summary>
    /// The volunteer (user) who registered.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
