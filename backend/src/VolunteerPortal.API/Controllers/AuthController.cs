using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunteerPortal.API.Application.Auth.Commands;
using VolunteerPortal.API.Application.Auth.Queries;
using VolunteerPortal.API.Models.DTOs.Auth;

namespace VolunteerPortal.API.Controllers;

/// <summary>
/// Authentication endpoints for user registration and login.
/// Handles user authentication, registration, and profile retrieval.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR mediator for dispatching commands and queries.</param>
    /// <param name="logger">Logger instance.</param>
    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    /// <remarks>
    /// Creates a new user account with the provided credentials.
    /// Email addresses must be unique in the system.
    /// Password must meet complexity requirements (minimum 8 characters).
    /// Returns a JWT token on successful registration.
    /// </remarks>
    /// <param name="request">Registration details including name, email, and password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication response with JWT token and user details.</returns>
    /// <response code="201">User registered successfully.</response>
    /// <response code="400">Invalid request data or validation error.</response>
    /// <response code="409">Email address is already in use.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(request.Email, request.Password, request.Name, request.PhoneNumber);
        var response = await _mediator.Send(command, cancellationToken);
        _logger.LogInformation("User registered successfully: {Email}", request.Email);
        return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    /// <remarks>
    /// Authenticates a user with their email and password.
    /// Returns a JWT token valid for the configured token lifetime.
    /// </remarks>
    /// <param name="request">Login credentials (email and password).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication response with JWT token and user details.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(request.Email, request.Password);
        var response = await _mediator.Send(command, cancellationToken);
        _logger.LogInformation("User logged in successfully: {Email}", request.Email);
        return Ok(response);
    }

    /// <summary>
    /// Get current authenticated user's profile.
    /// </summary>
    /// <remarks>
    /// Returns the profile information for the currently authenticated user,
    /// including their skills and role information.
    /// Requires a valid JWT token in the Authorization header.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User profile information.</returns>
    /// <response code="200">Returns the user profile.</response>
    /// <response code="401">User is not authenticated or token is invalid.</response>
    /// <response code="404">User not found (account may have been deleted).</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var query = new GetCurrentUserQuery(userId);
        var response = await _mediator.Send(query, cancellationToken);
        return Ok(response);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) 
            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        
        if (claim == null || !int.TryParse(claim.Value, out var userId))
            throw new UnauthorizedAccessException("Invalid token");
        
        return userId;
    }
}
