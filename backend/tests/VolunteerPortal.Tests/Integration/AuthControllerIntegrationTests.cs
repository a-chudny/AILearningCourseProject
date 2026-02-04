using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.DTOs.Auth;
using VolunteerPortal.API.Models.Enums;
using Xunit;

namespace VolunteerPortal.Tests.Integration;

/// <summary>
/// Integration tests for Auth endpoints
/// </summary>
public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove PostgreSQL DbContext registration
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // Add In-Memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb_" + Guid.NewGuid());
                });
            });
        });

        _client = _factory.CreateClient();
        
        // Initialize database after factory is created
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Register_ValidRequest_Returns201Created()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "integration@example.com",
            Password = "Password123",
            Name = "Integration Test User",
            PhoneNumber = "9876543210"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(authResponse);
        Assert.Equal(request.Email, authResponse.Email);
        Assert.Equal(request.Name, authResponse.Name);
        Assert.Equal((int)UserRole.Volunteer, authResponse.Role);
        Assert.NotEmpty(authResponse.Token);
        Assert.True(authResponse.Id > 0);
    }

    [Fact]
    public async Task Register_WithoutPhoneNumber_Returns201Created()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "nophone@example.com",
            Password = "Password123",
            Name = "No Phone User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(authResponse);
        Assert.Equal(request.Email, authResponse.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409Conflict()
    {
        // Arrange
        var firstRequest = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Password = "Password123",
            Name = "First User"
        };

        var secondRequest = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Password = "DifferentPassword456",
            Name = "Second User"
        };

        // Act
        await _client.PostAsJsonAsync("/api/auth/register", firstRequest);
        var response = await _client.PostAsJsonAsync("/api/auth/register", secondRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Email already exists", content);
    }

    [Fact]
    public async Task Register_InvalidEmail_Returns400BadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "invalid-email",
            Password = "Password123",
            Name = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_PasswordTooShort_Returns400BadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "short@example.com",
            Password = "Pass1", // Only 5 characters
            Name = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_PasswordWithoutNumber_Returns400BadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "nonumber@example.com",
            Password = "PasswordOnly", // No number
            Name = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_EmptyName_Returns400BadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "noname@example.com",
            Password = "Password123",
            Name = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #region Login Integration Tests

    [Fact]
    public async Task Login_ValidCredentials_Returns200OK()
    {
        // Arrange - Register a user first
        var registerRequest = new RegisterRequest
        {
            Email = "logintest@example.com",
            Password = "Password123",
            Name = "Login Test User"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "logintest@example.com",
            Password = "Password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(authResponse);
        Assert.Equal(loginRequest.Email, authResponse.Email);
        Assert.Equal("Login Test User", authResponse.Name);
        Assert.NotEmpty(authResponse.Token);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401Unauthorized()
    {
        // Arrange - Register a user first
        var registerRequest = new RegisterRequest
        {
            Email = "wrongpasstest@example.com",
            Password = "CorrectPassword123",
            Name = "Test User"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "wrongpasstest@example.com",
            Password = "WrongPassword456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid email or password", content);
    }

    [Fact]
    public async Task Login_NonExistentEmail_Returns401Unauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid email or password", content);
    }

    [Fact]
    public async Task Login_EmailCaseInsensitive_Returns200OK()
    {
        // Arrange - Register a user
        var registerRequest = new RegisterRequest
        {
            Email = "casetest@example.com",
            Password = "Password123",
            Name = "Case Test User"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "CASETEST@EXAMPLE.COM", // Different case
            Password = "Password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(authResponse);
        Assert.Equal("casetest@example.com", authResponse.Email); // Original case from DB
    }

    [Fact]
    public async Task Login_InvalidEmailFormat_Returns400BadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "invalid-email",
            Password = "Password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_EmptyPassword_Returns400BadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region GetCurrentUser Integration Tests

    [Fact]
    public async Task GetMe_WithValidToken_Returns200OK()
    {
        // Arrange - Register and login to get token
        var registerRequest = new RegisterRequest
        {
            Email = "getme@example.com",
            Password = "Password123",
            Name = "Get Me User",
            PhoneNumber = "1234567890"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(registerContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Add Authorization header with JWT token
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token);

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<UserResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(userResponse);
        Assert.Equal(registerRequest.Email, userResponse.Email);
        Assert.Equal(registerRequest.Name, userResponse.Name);
        Assert.Equal(registerRequest.PhoneNumber, userResponse.PhoneNumber);
        Assert.Equal((int)UserRole.Volunteer, userResponse.Role);
        Assert.NotNull(userResponse.Skills);
    }

    [Fact]
    public async Task GetMe_WithoutToken_Returns401Unauthorized()
    {
        // Arrange - No token in header

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithInvalidToken_Returns401Unauthorized()
    {
        // Arrange - Invalid token
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid.token.here");

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_ReturnsEmptySkillsList()
    {
        // Arrange - Register user without skills
        var registerRequest = new RegisterRequest
        {
            Email = "noskills@example.com",
            Password = "Password123",
            Name = "No Skills User"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(registerContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token);

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<UserResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(userResponse);
        Assert.NotNull(userResponse.Skills);
        Assert.Empty(userResponse.Skills);
    }

    #endregion
}
