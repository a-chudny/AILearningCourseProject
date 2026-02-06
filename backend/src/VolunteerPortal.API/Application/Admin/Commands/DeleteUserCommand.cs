using MediatR;

namespace VolunteerPortal.API.Application.Admin.Commands;

/// <summary>
/// Command to soft delete a user.
/// </summary>
public sealed record DeleteUserCommand(
    int UserId,
    int CurrentUserId) : IRequest<DeleteUserResult>;

/// <summary>
/// Result of delete user operation.
/// </summary>
public sealed record DeleteUserResult(string Message);
