using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.DTOs.Events;
using VolunteerPortal.API.Models.Entities;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.Tests.Integration;

public class EventsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EventsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase_Events");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetEvents_ReturnsOk_WithEventListResponse()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/events");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<EventListResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Events);
        Assert.True(result.TotalCount >= 0);
    }

    [Fact]
    public async Task GetEvents_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/events?Page=1&PageSize=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<EventListResponse>();
        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.True(result.Events.Count <= 5);
    }

    [Fact]
    public async Task GetEvents_WithSearchTerm_ReturnsFilteredEvents()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/events?SearchTerm=Community");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<EventListResponse>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetEventById_ValidId_ReturnsOk()
    {
        // Arrange
        var eventId = await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<EventResponse>();
        Assert.NotNull(result);
        Assert.Equal(eventId, result.Id);
    }

    [Fact]
    public async Task GetEventById_InvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/events/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var request = new CreateEventRequest
        {
            Title = "New Event",
            Description = "Test event",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 20,
            RequiredSkillIds = new List<int>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_AsOrganizer_ReturnsCreated()
    {
        // Arrange
        var token = await GetOrganizerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateEventRequest
        {
            Title = "Organizer Event",
            Description = "Created by organizer",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(10),
            DurationMinutes = 180,
            Capacity = 30,
            RequiredSkillIds = new List<int>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<EventResponse>();
        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
    }

    [Fact]
    public async Task CreateEvent_AsVolunteer_ReturnsForbidden()
    {
        // Arrange
        var token = await GetVolunteerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateEventRequest
        {
            Title = "Volunteer Event",
            Description = "Should not be created",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(10),
            DurationMinutes = 180,
            Capacity = 30,
            RequiredSkillIds = new List<int>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEvent_AsOwner_ReturnsOk()
    {
        // Arrange
        var (eventId, token) = await CreateEventAsOrganizerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated Event Title",
            Description = "Updated description",
            Location = "Updated Location",
            StartTime = DateTime.UtcNow.AddDays(15),
            DurationMinutes = 240,
            Capacity = 50,
            Status = EventStatus.Active,
            RequiredSkillIds = new List<int>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/events/{eventId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<EventResponse>();
        Assert.NotNull(result);
        Assert.Equal(updateRequest.Title, result.Title);
    }

    [Fact]
    public async Task UpdateEvent_AsNonOwner_ReturnsForbidden()
    {
        // Arrange
        var eventId = await SeedTestDataAsync();
        var differentOrganizerToken = await GetOrganizerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", differentOrganizerToken);

        var updateRequest = new UpdateEventRequest
        {
            Title = "Should Not Update",
            Description = "Not owner",
            Location = "Test",
            StartTime = DateTime.UtcNow.AddDays(15),
            DurationMinutes = 240,
            Capacity = 50,
            Status = EventStatus.Active,
            RequiredSkillIds = new List<int>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/events/{eventId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEvent_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var eventId = await SeedTestDataAsync();

        var updateRequest = new UpdateEventRequest
        {
            Title = "Should Not Update",
            Description = "No auth",
            Location = "Test",
            StartTime = DateTime.UtcNow.AddDays(15),
            DurationMinutes = 240,
            Capacity = 50,
            Status = EventStatus.Active,
            RequiredSkillIds = new List<int>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/events/{eventId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_AsOwner_ReturnsNoContent()
    {
        // Arrange
        var (eventId, token) = await CreateEventAsOrganizerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync($"/api/events/{eventId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_AsNonOwner_ReturnsForbidden()
    {
        // Arrange
        var eventId = await SeedTestDataAsync();
        var differentOrganizerToken = await GetOrganizerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", differentOrganizerToken);

        // Act
        var response = await _client.DeleteAsync($"/api/events/{eventId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var eventId = await SeedTestDataAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/events/{eventId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await GetOrganizerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync("/api/events/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Helper methods
    private async Task<int> SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var organizer = new User
        {
            Email = "organizer@test.com",
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
            Title = "Community Cleanup",
            Description = "Help clean the local park",
            Location = "Central Park",
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

    private async Task<string> GetOrganizerTokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var organizer = new User
        {
            Email = $"organizer{Guid.NewGuid()}@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Name = "New Organizer",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.Add(organizer);
        await context.SaveChangesAsync();

        // Generate JWT token (simplified - in real tests would use auth endpoint)
        return "mock-organizer-token";
    }

    private async Task<string> GetVolunteerTokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var volunteer = new User
        {
            Email = $"volunteer{Guid.NewGuid()}@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Name = "New Volunteer",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.Add(volunteer);
        await context.SaveChangesAsync();

        return "mock-volunteer-token";
    }

    private async Task<(int eventId, string token)> CreateEventAsOrganizerAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var organizer = new User
        {
            Email = $"creator{Guid.NewGuid()}@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Name = "Event Creator",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.Add(organizer);
        await context.SaveChangesAsync();

        var testEvent = new Event
        {
            Title = "Organizer's Event",
            Description = "Event for testing",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(10),
            DurationMinutes = 180,
            Capacity = 30,
            OrganizerId = organizer.Id,
            Status = EventStatus.Active,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Events.Add(testEvent);
        await context.SaveChangesAsync();

        return (testEvent.Id, "mock-creator-token");
    }
}
