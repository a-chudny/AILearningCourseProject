using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VolunteerPortal.API.Models.Entities;

/// <summary>
/// Join entity representing the many-to-many relationship between Events and Skills.
/// </summary>
public class EventSkill
{
    /// <summary>
    /// Foreign key to the event.
    /// </summary>
    [Required]
    public int EventId { get; set; }

    /// <summary>
    /// Foreign key to the skill.
    /// </summary>
    [Required]
    public int SkillId { get; set; }

    /// <summary>
    /// Indicates if this skill is required (true) or optional (false) for the event.
    /// </summary>
    [Required]
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Timestamp when this skill requirement was added to the event.
    /// </summary>
    [Required]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    /// <summary>
    /// The event that requires or prefers this skill.
    /// </summary>
    [ForeignKey(nameof(EventId))]
    public Event Event { get; set; } = null!;

    /// <summary>
    /// The skill required or preferred for the event.
    /// </summary>
    [ForeignKey(nameof(SkillId))]
    public Skill Skill { get; set; } = null!;
}
