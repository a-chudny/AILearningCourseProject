namespace VolunteerPortal.API.Models.DTOs;

/// <summary>
/// Response model for skill information.
/// </summary>
public class SkillResponse
{
    /// <summary>
    /// Unique identifier for the skill.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the skill.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category of the skill (e.g., Medical, Education, Technology).
    /// </summary>
    public string Category { get; set; } = string.Empty;
}
