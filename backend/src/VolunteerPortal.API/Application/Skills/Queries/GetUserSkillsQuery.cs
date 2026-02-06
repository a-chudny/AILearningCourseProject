using MediatR;
using VolunteerPortal.API.Models.DTOs;

namespace VolunteerPortal.API.Application.Skills.Queries;

/// <summary>
/// Query to get skills for a specific user.
/// </summary>
/// <param name="UserId">The ID of the user.</param>
public sealed record GetUserSkillsQuery(int UserId) : IRequest<List<SkillResponse>>;
