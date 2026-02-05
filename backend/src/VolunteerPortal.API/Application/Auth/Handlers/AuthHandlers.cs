using MediatR;
using VolunteerPortal.API.Application.Auth.Commands;
using VolunteerPortal.API.Application.Auth.Queries;
using VolunteerPortal.API.Models.DTOs.Auth;
using VolunteerPortal.API.Services.Interfaces;

namespace VolunteerPortal.API.Application.Auth.Handlers;

/// <summary>
/// Handler for user registration command.
/// </summary>
public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IAuthService _authService;

    public RegisterUserHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var registerRequest = new RegisterRequest
        {
            Email = request.Email,
            Password = request.Password,
            Name = request.Name,
            PhoneNumber = request.PhoneNumber
        };

        return await _authService.RegisterAsync(registerRequest, cancellationToken);
    }
}

/// <summary>
/// Handler for user login command.
/// </summary>
public sealed class LoginUserHandler : IRequestHandler<LoginUserCommand, AuthResponse>
{
    private readonly IAuthService _authService;

    public LoginUserHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequest
        {
            Email = request.Email,
            Password = request.Password
        };

        return await _authService.LoginAsync(loginRequest, cancellationToken);
    }
}

/// <summary>
/// Handler for getting current user profile query.
/// </summary>
public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, UserResponse>
{
    private readonly IAuthService _authService;

    public GetCurrentUserHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<UserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        return await _authService.GetCurrentUserAsync(request.UserId, cancellationToken);
    }
}
