using MediatR;

namespace VolunteerPortal.API.Application.Skills.Commands;

/// <summary>
/// Command to update a user's skills.
/// </summary>
/// <param name="UserId">The ID of the user.</param>
/// <param name="SkillIds">List of skill IDs to assign to the user.</param>
public sealed record UpdateUserSkillsCommand(int UserId, List<int> SkillIds) : IRequest<Unit>;
