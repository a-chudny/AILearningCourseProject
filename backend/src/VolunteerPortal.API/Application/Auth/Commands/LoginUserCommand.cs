using MediatR;
using VolunteerPortal.API.Models.DTOs.Auth;

namespace VolunteerPortal.API.Application.Auth.Commands;

/// <summary>
/// Command to authenticate a user with email and password.
/// </summary>
/// <param name="Email">User's email address.</param>
/// <param name="Password">User's password.</param>
public sealed record LoginUserCommand(string Email, string Password) : IRequest<AuthResponse>;
