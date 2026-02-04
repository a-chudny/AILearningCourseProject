using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.DTOs;
using VolunteerPortal.API.Models.DTOs.Auth;
using VolunteerPortal.API.Models.Entities;
using VolunteerPortal.API.Models.Enums;
using VolunteerPortal.API.Services.Interfaces;
using BCryptNet = BCrypt.Net.BCrypt;

namespace VolunteerPortal.API.Services;

/// <summary>
/// Service for authentication operations
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Check if email already exists
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted, cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Hash password with BCrypt
        var passwordHash = BCryptNet.HashPassword(request.Password);

        // Create new user with Volunteer role
        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = passwordHash,
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User registered successfully: {Email}", user.Email);

        // Generate JWT token
        var token = GenerateJwtToken(user);

        // Return authentication response
        return new AuthResponse
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = (int)user.Role,
            Token = token
        };
    }

    /// <inheritdoc />
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Find user by email (case-insensitive), exclude soft-deleted users
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted, cancellationToken);

        // Return same error message for user not found or wrong password (security best practice)
        if (user == null)
        {
            _logger.LogWarning("Login attempt with non-existent or deleted email: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Verify password with BCrypt
        var isPasswordValid = BCryptNet.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            _logger.LogWarning("Login attempt with invalid password for user: {Email}", user.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        _logger.LogInformation("User logged in successfully: {Email}", user.Email);

        // Generate JWT token
        var token = GenerateJwtToken(user);

        // Return authentication response
        return new AuthResponse
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = (int)user.Role,
            Token = token
        };
    }

    /// <summary>
    /// Generate JWT token for authenticated user
    /// </summary>
    private string GenerateJwtToken(User user)
    {
        var jwtSecret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is not configured");
        var jwtAudience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT Audience is not configured");
        var jwtExpirationHours = _configuration.GetValue<int>("Jwt:ExpirationInHours", 24);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(jwtExpirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public async Task<UserResponse> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Find user with skills
        var user = await _context.Users
            .Include(u => u.UserSkills)
                .ThenInclude(us => us.Skill)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        _logger.LogInformation("Retrieved current user profile: {UserId}", userId);

        // Map to response
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            PhoneNumber = user.PhoneNumber,
            Role = (int)user.Role,
            Skills = user.UserSkills
                .Select(us => new SkillResponse
                {
                    Id = us.Skill.Id,
                    Name = us.Skill.Name,
                    Category = us.Skill.Description ?? string.Empty
                })
                .ToList()
        };
    }
}
