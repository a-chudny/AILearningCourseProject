using System.ComponentModel.DataAnnotations;

namespace VolunteerPortal.API.Models.DTOs.Skills;

/// <summary>
/// Request model for updating user's skills.
/// </summary>
public class UpdateUserSkillsRequest
{
    /// <summary>
    /// List of skill IDs to assign to the user.
    /// Replaces all existing skills.
    /// </summary>
    [Required]
    public List<int> SkillIds { get; set; } = new();
}
