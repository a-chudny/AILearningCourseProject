using MediatR;
using VolunteerPortal.API.Models.DTOs.Auth;

namespace VolunteerPortal.API.Application.Auth.Queries;

/// <summary>
/// Query to get the current authenticated user's profile.
/// </summary>
/// <param name="UserId">The ID of the current user from JWT claims.</param>
public sealed record GetCurrentUserQuery(int UserId) : IRequest<UserResponse>;
