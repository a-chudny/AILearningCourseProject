using MediatR;
using VolunteerPortal.API.Application.Admin.Models;

namespace VolunteerPortal.API.Application.Admin.Queries;

/// <summary>
/// Query to get admin dashboard statistics.
/// </summary>
public sealed record GetAdminStatsQuery : IRequest<AdminStatsModel>;
