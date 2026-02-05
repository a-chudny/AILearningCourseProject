using MediatR;
using VolunteerPortal.API.Application.Admin.Models;
using VolunteerPortal.API.Application.Common;

namespace VolunteerPortal.API.Application.Admin.Queries;

/// <summary>
/// Query to get paginated list of users for admin management.
/// </summary>
public sealed record GetUsersQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    bool IncludeDeleted = true,
    string? Status = null) : IRequest<PagedResult<AdminUserModel>>;
