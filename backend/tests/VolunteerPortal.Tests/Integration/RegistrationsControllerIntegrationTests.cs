using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.DTOs.Auth;
using VolunteerPortal.API.Models.DTOs.Registrations;
using VolunteerPortal.API.Models.Entities;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.Tests.Integration;

/// <summary>
/// Integration tests for Registration endpoints.
/// Tests complete registration flow including registration, cancellation, and listing.
/// </summary>
public class RegistrationsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RegistrationsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.EnsureDatabaseCreated();
        _client = _factory.CreateClient();
    }

    #region Register for Event Tests

    [Fact]
    public async Task RegisterForEvent_AsVolunteer_ReturnsCreated()
    {
        // Arrange
        var (eventId, volunteerToken) = await CreateEventWithVolunteerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteerToken);

        // Act
        var response = await _client.PostAsync($"/api/events/{eventId}/register", null);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var registration = JsonSerializer.Deserialize<RegistrationResponse>(content, JsonOptions);

        Assert.NotNull(registration);
        Assert.Equal(eventId, registration.EventId);
        Assert.Equal(RegistrationStatus.Confirmed, registration.Status);
    }

    [Fact]
    public async Task RegisterForEvent_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var eventId = await CreateTestEventAsync();
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.PostAsync($"/api/events/{eventId}/register", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterForEvent_Twice_ReturnsConflict()
    {
        // Arrange
        var (eventId, volunteerToken) = await CreateEventWithVolunteerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteerToken);

        // First registration
        await _client.PostAsync($"/api/events/{eventId}/register", null);

        // Act - Try to register again
        var response = await _client.PostAsync($"/api/events/{eventId}/register", null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task RegisterForEvent_NonExistentEvent_ReturnsNotFound()
    {
        // Arrange
        var token = await GetVolunteerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/events/99999/register", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RegisterForEvent_AtCapacity_ReturnsConflict()
    {
        // Arrange
        var eventId = await CreateEventWithCapacityAsync(1); // Only 1 spot

        // First volunteer registers
        var token1 = await GetVolunteerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        await _client.PostAsync($"/api/events/{eventId}/register", null);

        // Second volunteer tries to register
        var token2 = await GetVolunteerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await _client.PostAsync($"/api/events/{eventId}/register", null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task RegisterForEvent_CancelledEvent_ReturnsBadRequest()
    {
        // Arrange
        var eventId = await CreateCancelledEventAsync();
        var token = await GetVolunteerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync($"/api/events/{eventId}/register", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Cancel Registration Tests

    [Fact]
    public async Task CancelRegistration_ExistingRegistration_ReturnsNoContent()
    {
        // Arrange
        var (eventId, volunteerToken) = await CreateEventWithVolunteerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteerToken);

        // First register
        await _client.PostAsync($"/api/events/{eventId}/register", null);

        // Act
        var response = await _client.DeleteAsync($"/api/events/{eventId}/register");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CancelRegistration_NotRegistered_ReturnsNotFound()
    {
        // Arrange
        var eventId = await CreateTestEventAsync();
        var token = await GetVolunteerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - Cancel without registering first
        var response = await _client.DeleteAsync($"/api/events/{eventId}/register");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CancelRegistration_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var eventId = await CreateTestEventAsync();
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.DeleteAsync($"/api/events/{eventId}/register");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Get User Registrations Tests

    [Fact]
    public async Task GetUserRegistrations_Authenticated_ReturnsOk()
    {
        // Arrange
        var (eventId, volunteerToken) = await CreateEventWithVolunteerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteerToken);

        // Register for event
        await _client.PostAsync($"/api/events/{eventId}/register", null);

        // Act
        var response = await _client.GetAsync("/api/users/me/registrations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var registrations = JsonSerializer.Deserialize<List<RegistrationResponse>>(content, JsonOptions);

        Assert.NotNull(registrations);
        Assert.Contains(registrations, r => r.EventId == eventId);
    }

    [Fact]
    public async Task GetUserRegistrations_NoRegistrations_ReturnsEmptyList()
    {
        // Arrange
        var token = await GetVolunteerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/users/me/registrations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var registrations = JsonSerializer.Deserialize<List<RegistrationResponse>>(content, JsonOptions);

        Assert.NotNull(registrations);
        Assert.Empty(registrations);
    }

    [Fact]
    public async Task GetUserRegistrations_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync("/api/users/me/registrations");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Get Event Registrations Tests (Organizer View)

    [Fact]
    public async Task GetEventRegistrations_AsOrganizer_ReturnsOk()
    {
        // Arrange
        var (eventId, organizerToken, _) = await CreateEventWithRegistrationAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", organizerToken);

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}/registrations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var registrations = JsonSerializer.Deserialize<List<EventRegistrationResponse>>(content, JsonOptions);

        Assert.NotNull(registrations);
        Assert.Single(registrations);
        Assert.NotNull(registrations[0].User);
    }

    [Fact]
    public async Task GetEventRegistrations_AsAdmin_ReturnsOk()
    {
        // Arrange
        var eventId = await CreateTestEventAsync();
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}/registrations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetEventRegistrations_AsVolunteer_ReturnsForbidden()
    {
        // Arrange
        var eventId = await CreateTestEventAsync();
        var volunteerToken = await GetVolunteerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteerToken);

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}/registrations");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetEventRegistrations_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var eventId = await CreateTestEventAsync();
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}/registrations");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Complete Registration Flow Tests

    [Fact]
    public async Task CompleteFlow_RegisterThenCancel_ThenReregister()
    {
        // Arrange
        var (eventId, volunteerToken) = await CreateEventWithVolunteerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteerToken);

        // Act 1 - Register
        var registerResponse = await _client.PostAsync($"/api/events/{eventId}/register", null);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        // Act 2 - Cancel
        var cancelResponse = await _client.DeleteAsync($"/api/events/{eventId}/register");
        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        // Act 3 - Re-register
        var reregisterResponse = await _client.PostAsync($"/api/events/{eventId}/register", null);

        // Assert - Can re-register after cancellation
        Assert.Equal(HttpStatusCode.Created, reregisterResponse.StatusCode);
    }

    [Fact]
    public async Task CompleteFlow_RegisterAndVerifyInUserList()
    {
        // Arrange
        var (eventId, volunteerToken) = await CreateEventWithVolunteerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteerToken);

        // Act 1 - Register
        await _client.PostAsync($"/api/events/{eventId}/register", null);

        // Act 2 - Get user registrations
        var listResponse = await _client.GetAsync("/api/users/me/registrations");
        var content = await listResponse.Content.ReadAsStringAsync();
        var registrations = JsonSerializer.Deserialize<List<RegistrationResponse>>(content, JsonOptions);

        // Assert
        Assert.NotNull(registrations);
        Assert.Single(registrations);
        Assert.Equal(eventId, registrations[0].EventId);
        Assert.Equal(RegistrationStatus.Confirmed, registrations[0].Status);
        Assert.NotNull(registrations[0].Event);
    }

    #endregion

    #region Helper Methods

    private async Task<int> CreateTestEventAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var organizer = new User
        {
            Email = $"organizer{Guid.NewGuid()}@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Name = "Test Organizer",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.Add(organizer);
        await context.SaveChangesAsync();

        var testEvent = new Event
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 20,
            OrganizerId = organizer.Id,
            Status = EventStatus.Active,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Events.Add(testEvent);
        await context.SaveChangesAsync();

        return testEvent.Id;
    }

    private async Task<int> CreateEventWithCapacityAsync(int capacity)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var organizer = new User
        {
            Email = $"organizer{Guid.NewGuid()}@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Name = "Test Organizer",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.Add(organizer);
        await context.SaveChangesAsync();

        var testEvent = new Event
        {
            Title = "Limited Capacity Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = capacity,
            OrganizerId = organizer.Id,
            Status = EventStatus.Active,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Events.Add(testEvent);
        await context.SaveChangesAsync();

        return testEvent.Id;
    }

    private async Task<int> CreateCancelledEventAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var organizer = new User
        {
            Email = $"organizer{Guid.NewGuid()}@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Name = "Test Organizer",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.Add(organizer);
        await context.SaveChangesAsync();

        var testEvent = new Event
        {
            Title = "Cancelled Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 20,
            OrganizerId = organizer.Id,
            Status = EventStatus.Cancelled,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Events.Add(testEvent);
        await context.SaveChangesAsync();

        return testEvent.Id;
    }

    private async Task<(int eventId, string volunteerToken)> CreateEventWithVolunteerAsync()
    {
        var eventId = await CreateTestEventAsync();
        var volunteerToken = await GetVolunteerTokenAsync();
        return (eventId, volunteerToken);
    }

    private async Task<(int eventId, string organizerToken, string volunteerToken)> CreateEventWithRegistrationAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var organizerEmail = $"organizer{Guid.NewGuid()}@test.com";
        var volunteerEmail = $"volunteer{Guid.NewGuid()}@test.com";
        var password = "Password123";

        var organizer = new User
        {
            Email = organizerEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Name = "Test Organizer",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var volunteer = new User
        {
            Email = volunteerEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Name = "Test Volunteer",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.AddRange(organizer, volunteer);
        await context.SaveChangesAsync();

        var testEvent = new Event
        {
            Title = "Event With Registration",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 20,
            OrganizerId = organizer.Id,
            Status = EventStatus.Active,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Events.Add(testEvent);
        await context.SaveChangesAsync();

        // Create a registration
        var registration = new Registration
        {
            EventId = testEvent.Id,
            UserId = volunteer.Id,
            Status = RegistrationStatus.Confirmed,
            RegisteredAt = DateTime.UtcNow
        };

        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Get tokens
        var organizerToken = await LoginAndGetTokenAsync(organizerEmail, password);
        var volunteerToken = await LoginAndGetTokenAsync(volunteerEmail, password);

        return (testEvent.Id, organizerToken, volunteerToken);
    }

    private async Task<string> GetVolunteerTokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var email = $"volunteer{Guid.NewGuid()}@test.com";
        var password = "Password123";

        var volunteer = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Name = "Test Volunteer",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.Add(volunteer);
        await context.SaveChangesAsync();

        return await LoginAndGetTokenAsync(email, password);
    }

    private async Task<string> GetAdminTokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var email = $"admin{Guid.NewGuid()}@test.com";
        var password = "Password123";

        var admin = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Name = "Test Admin",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();

        return await LoginAndGetTokenAsync(email, password);
    }

    private async Task<string> LoginAndGetTokenAsync(string email, string password)
    {
        var loginRequest = new LoginRequest { Email = email, Password = password };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, JsonOptions);
        return authResponse!.Token;
    }

    #endregion
}
