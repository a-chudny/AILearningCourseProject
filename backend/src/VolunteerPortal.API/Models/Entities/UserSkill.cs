using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VolunteerPortal.API.Models.Entities;

/// <summary>
/// Join entity representing the many-to-many relationship between Users and Skills.
/// </summary>
public class UserSkill
{
    /// <summary>
    /// Foreign key to the user.
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Foreign key to the skill.
    /// </summary>
    [Required]
    public int SkillId { get; set; }

    /// <summary>
    /// Optional proficiency level or years of experience.
    /// </summary>
    [MaxLength(100)]
    public string? ProficiencyLevel { get; set; }

    /// <summary>
    /// Timestamp when this skill was added to the user's profile.
    /// </summary>
    [Required]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    /// <summary>
    /// The user who has this skill.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    /// <summary>
    /// The skill associated with the user.
    /// </summary>
    [ForeignKey(nameof(SkillId))]
    public Skill Skill { get; set; } = null!;
}
