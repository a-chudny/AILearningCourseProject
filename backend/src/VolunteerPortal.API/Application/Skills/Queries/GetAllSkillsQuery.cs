using MediatR;
using VolunteerPortal.API.Models.DTOs;

namespace VolunteerPortal.API.Application.Skills.Queries;

/// <summary>
/// Query to get all available skills.
/// </summary>
public sealed record GetAllSkillsQuery : IRequest<List<SkillResponse>>;
