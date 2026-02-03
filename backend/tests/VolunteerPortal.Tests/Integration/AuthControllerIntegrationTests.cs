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
}
