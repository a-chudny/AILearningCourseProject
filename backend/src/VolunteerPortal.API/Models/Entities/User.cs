using System.ComponentModel.DataAnnotations;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Models.Entities;

/// <summary>
/// Represents a user in the volunteer portal system.
/// Can be a volunteer, organizer, or admin.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// User's full name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User's email address (unique across the system).
    /// </summary>
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password for authentication.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User's role in the system.
    /// </summary>
    [Required]
    public UserRole Role { get; set; } = UserRole.Volunteer;

    /// <summary>
    /// Optional phone number for contact.
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Timestamp when the user account was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the user account was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete flag. When true, the user is marked as deleted.
    /// </summary>
    [Required]
    public bool IsDeleted { get; set; } = false;

    // Navigation properties

    /// <summary>
    /// Events created by this user (if organizer or admin).
    /// </summary>
    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();

    /// <summary>
    /// Event registrations for this user (if volunteer).
    /// </summary>
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    /// <summary>
    /// Skills associated with this user through join table.
    /// </summary>
    public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
}
