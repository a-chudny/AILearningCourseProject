using MediatR;
using VolunteerPortal.API.Application.Admin.Models;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Application.Admin.Commands;

/// <summary>
/// Command to update a user's role.
/// </summary>
public sealed record UpdateUserRoleCommand(
    int UserId,
    UserRole NewRole,
    int CurrentUserId) : IRequest<AdminUserModel>;
