using VolunteerPortal.API.Models.DTOs;

namespace VolunteerPortal.API.Services.Interfaces;

/// <summary>
/// Service interface for managing skills and user skill associations.
/// </summary>
public interface ISkillService
{
    /// <summary>
    /// Retrieves all available skills in the system.
    /// </summary>
    /// <returns>List of all skills.</returns>
    Task<List<SkillResponse>> GetAllSkillsAsync();

    /// <summary>
    /// Retrieves all skills associated with a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>List of user's skills.</returns>
    Task<List<SkillResponse>> GetUserSkillsAsync(int userId);

    /// <summary>
    /// Updates a user's skills by replacing all existing skills with new ones.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="skillIds">List of skill IDs to assign to the user.</param>
    /// <exception cref="ArgumentException">Thrown when one or more skill IDs are invalid.</exception>
    Task UpdateUserSkillsAsync(int userId, List<int> skillIds);
}
