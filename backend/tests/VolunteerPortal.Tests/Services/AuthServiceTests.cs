using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.DTOs.Auth;
using VolunteerPortal.API.Models.Enums;
using VolunteerPortal.API.Services;
using Xunit;

namespace VolunteerPortal.Tests.Services;

/// <summary>
/// Unit tests for AuthService
/// </summary>
public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Set up in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Set up configuration with JWT settings
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:Secret", "TestSecretKeyThatIsAtLeast32CharactersLongForTesting123456"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpirationInHours", "24"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _loggerMock = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(_context, _configuration, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123",
            Name = "Test User",
            PhoneNumber = "1234567890"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal((int)UserRole.Volunteer, result.Role);
        Assert.NotEmpty(result.Token);
        Assert.True(result.Id > 0);

        // Verify user was created in database
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        Assert.NotNull(user);
        Assert.NotEqual(request.Password, user.PasswordHash); // Password should be hashed
        Assert.False(user.IsDeleted);
    }

    [Fact]
    public async Task RegisterAsync_WithoutPhoneNumber_ReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "nophone@example.com",
            Password = "Password123",
            Name = "No Phone User"
            // PhoneNumber not provided
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.NotEmpty(result.Token);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        Assert.NotNull(user);
        Assert.Null(user.PhoneNumber);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingUser = new VolunteerPortal.API.Models.Entities.User
        {
            Email = "existing@example.com",
            Name = "Existing User",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123",
            Name = "New User"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.RegisterAsync(request));

        Assert.Equal("Email already exists", exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_EmailCaseInsensitive_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingUser = new VolunteerPortal.API.Models.Entities.User
        {
            Email = "test@example.com",
            Name = "Existing User",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Email = "TEST@EXAMPLE.COM", // Different case
            Password = "Password123",
            Name = "New User"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.RegisterAsync(request));

        Assert.Equal("Email already exists", exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_DeletedUserEmail_AllowsRegistration()
    {
        // Arrange
        var deletedUser = new VolunteerPortal.API.Models.Entities.User
        {
            Email = "deleted@example.com",
            Name = "Deleted User",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = true // User was deleted
        };

        _context.Users.Add(deletedUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Email = "deleted@example.com",
            Password = "Password123",
            Name = "New User"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task RegisterAsync_SetsVolunteerRoleByDefault()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "volunteer@example.com",
            Password = "Password123",
            Name = "Volunteer User"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.Equal((int)UserRole.Volunteer, result.Role);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        Assert.NotNull(user);
        Assert.Equal(UserRole.Volunteer, user.Role);
    }

    [Fact]
    public async Task RegisterAsync_GeneratesValidJwtToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "jwt@example.com",
            Password = "Password123",
            Name = "JWT User"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotEmpty(result.Token);
        // JWT token format: header.payload.signature
        var tokenParts = result.Token.Split('.');
        Assert.Equal(3, tokenParts.Length);
    }

    [Fact]
    public async Task RegisterAsync_HashesPassword()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "hash@example.com",
            Password = "Password123",
            Name = "Hash User"
        };

        // Act
        await _authService.RegisterAsync(request);

        // Assert
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        Assert.NotNull(user);
        Assert.NotEqual(request.Password, user.PasswordHash);
        Assert.NotEmpty(user.PasswordHash);

        // Verify BCrypt hash format (starts with $2a$ or similar)
        Assert.Matches(@"^\$2[ayb]\$\d{2}\$", user.PasswordHash);
    }

    #region Login Tests

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange - Create a user first
        var registerRequest = new RegisterRequest
        {
            Email = "login@example.com",
            Password = "Password123",
            Name = "Login User"
        };
        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "login@example.com",
            Password = "Password123"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(loginRequest.Email, result.Email);
        Assert.Equal("Login User", result.Name);
        Assert.Equal((int)UserRole.Volunteer, result.Role);
        Assert.NotEmpty(result.Token);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange - Create a user first
        var registerRequest = new RegisterRequest
        {
            Email = "wrongpass@example.com",
            Password = "CorrectPassword123",
            Name = "Test User"
        };
        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "wrongpass@example.com",
            Password = "WrongPassword123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(loginRequest));

        Assert.Equal("Invalid email or password", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_NonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(loginRequest));

        Assert.Equal("Invalid email or password", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_DeletedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange - Create a user and mark as deleted
        var user = new VolunteerPortal.API.Models.Entities.User
        {
            Email = "deleted@example.com",
            Name = "Deleted User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "deleted@example.com",
            Password = "Password123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(loginRequest));

        Assert.Equal("Invalid email or password", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_EmailCaseInsensitive_ReturnsAuthResponse()
    {
        // Arrange - Create a user
        var registerRequest = new RegisterRequest
        {
            Email = "case@example.com",
            Password = "Password123",
            Name = "Case User"
        };
        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "CASE@EXAMPLE.COM", // Different case
            Password = "Password123"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("case@example.com", result.Email); // Original case from DB
    }

    [Fact]
    public async Task LoginAsync_GeneratesValidJwtToken()
    {
        // Arrange - Create a user
        var registerRequest = new RegisterRequest
        {
            Email = "jwtlogin@example.com",
            Password = "Password123",
            Name = "JWT Login User"
        };
        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "jwtlogin@example.com",
            Password = "Password123"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.NotEmpty(result.Token);
        // JWT token format: header.payload.signature
        var tokenParts = result.Token.Split('.');
        Assert.Equal(3, tokenParts.Length);
    }

    #endregion
}
