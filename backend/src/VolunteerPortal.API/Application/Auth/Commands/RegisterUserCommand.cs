using MediatR;
using VolunteerPortal.API.Models.DTOs.Auth;

namespace VolunteerPortal.API.Application.Auth.Commands;

/// <summary>
/// Command to register a new user account.
/// </summary>
/// <param name="Email">User's email address.</param>
/// <param name="Password">User's password.</param>
/// <param name="Name">User's display name.</param>
/// <param name="PhoneNumber">Optional phone number.</param>
public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string Name,
    string? PhoneNumber = null) : IRequest<AuthResponse>;
