using System.ComponentModel.DataAnnotations;

namespace VolunteerPortal.API.Models.Entities;

/// <summary>
/// Represents a skill that can be associated with users and events.
/// </summary>
public class Skill
{
    /// <summary>
    /// Unique identifier for the skill.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Name of the skill (e.g., "First Aid", "Cooking", "Event Planning").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the skill.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Timestamp when the skill was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    /// <summary>
    /// Users who have this skill through join table.
    /// </summary>
    public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();

    /// <summary>
    /// Events that require this skill through join table.
    /// </summary>
    public ICollection<EventSkill> EventSkills { get; set; } = new List<EventSkill>();
}
