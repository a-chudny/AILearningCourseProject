using VolunteerPortal.API.Models.DTOs.Auth;

namespace VolunteerPortal.API.Services.Interfaces;

/// <summary>
/// Service for authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Register a new user with Volunteer role and return authentication response with JWT token
    /// </summary>
    /// <param name="request">Registration request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with user info and JWT token</returns>
    /// <exception cref="InvalidOperationException">Thrown when email already exists</exception>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticate user with email and password and return JWT token
    /// </summary>
    /// <param name="request">Login request with email and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with user info and JWT token</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid or user is deleted</exception>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current authenticated user's profile information including skills
    /// </summary>
    /// <param name="userId">User ID from JWT claims</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile with skills list</returns>
    /// <exception cref="InvalidOperationException">Thrown when user not found</exception>
    Task<UserResponse> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default);
}
