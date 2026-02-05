using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.DTOs.Auth;
using VolunteerPortal.API.Models.Entities;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.Tests.Integration;

/// <summary>
/// Shared test helpers for integration tests.
/// Provides common methods for user creation, authentication, and test data seeding.
/// </summary>
public static class IntegrationTestHelpers
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    #region User Creation Helpers

    /// <summary>
    /// Creates a user directly in the database and returns their ID.
    /// </summary>
    public static async Task<int> CreateUserAsync(
        IServiceProvider services,
        string? email = null,
        string password = "Password123",
        string name = "Test User",
        UserRole role = UserRole.Volunteer,
        bool isDeleted = false)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        email ??= $"{role.ToString().ToLower()}{Guid.NewGuid()}@test.com";

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Name = name,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = isDeleted
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.Id;
    }

    /// <summary>
    /// Creates a user and returns their ID along with credentials.
    /// </summary>
    public static async Task<(int userId, string email, string password)> CreateUserWithCredentialsAsync(
        IServiceProvider services,
        UserRole role = UserRole.Volunteer,
        string name = "Test User")
    {
        var email = $"{role.ToString().ToLower()}{Guid.NewGuid()}@test.com";
        var password = "Password123";

        var userId = await CreateUserAsync(services, email, password, name, role);
        return (userId, email, password);
    }

    #endregion

    #region Authentication Helpers

    /// <summary>
    /// Logs in a user and returns their JWT token.
    /// </summary>
    public static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var loginRequest = new LoginRequest { Email = email, Password = password };
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, JsonOptions);
        return authResponse!.Token;
    }

    /// <summary>
    /// Creates a user and returns their JWT token.
    /// </summary>
    public static async Task<string> CreateAndAuthenticateUserAsync(
        IServiceProvider services,
        HttpClient client,
        UserRole role = UserRole.Volunteer,
        string name = "Test User")
    {
        var (_, email, password) = await CreateUserWithCredentialsAsync(services, role, name);
        return await LoginAsync(client, email, password);
    }

    /// <summary>
    /// Creates a user and returns their ID and JWT token.
    /// </summary>
    public static async Task<(int userId, string token)> CreateAndAuthenticateUserWithIdAsync(
        IServiceProvider services,
        HttpClient client,
        UserRole role = UserRole.Volunteer,
        string name = "Test User")
    {
        var (userId, email, password) = await CreateUserWithCredentialsAsync(services, role, name);
        var token = await LoginAsync(client, email, password);
        return (userId, token);
    }

    /// <summary>
    /// Sets the authorization header on an HttpClient.
    /// </summary>
    public static void SetAuthToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Clears the authorization header from an HttpClient.
    /// </summary>
    public static void ClearAuth(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region Event Creation Helpers

    /// <summary>
    /// Creates an event and returns its ID.
    /// </summary>
    public static async Task<int> CreateEventAsync(
        IServiceProvider services,
        int organizerId,
        string title = "Test Event",
        EventStatus status = EventStatus.Active,
        int capacity = 20,
        int daysFromNow = 7)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var testEvent = new Event
        {
            Title = title,
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(daysFromNow),
            DurationMinutes = 120,
            Capacity = capacity,
            OrganizerId = organizerId,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Events.Add(testEvent);
        await context.SaveChangesAsync();

        return testEvent.Id;
    }

    /// <summary>
    /// Creates an organizer with an event and returns both IDs along with organizer token.
    /// </summary>
    public static async Task<(int organizerId, int eventId, string organizerToken)> CreateOrganizerWithEventAsync(
        IServiceProvider services,
        HttpClient client,
        string eventTitle = "Test Event",
        int capacity = 20)
    {
        var (organizerId, email, password) = await CreateUserWithCredentialsAsync(services, UserRole.Organizer, "Test Organizer");
        var eventId = await CreateEventAsync(services, organizerId, eventTitle, EventStatus.Active, capacity);
        var token = await LoginAsync(client, email, password);
        return (organizerId, eventId, token);
    }

    #endregion

    #region Registration Helpers

    /// <summary>
    /// Creates a registration directly in the database.
    /// </summary>
    public static async Task<int> CreateRegistrationAsync(
        IServiceProvider services,
        int eventId,
        int userId,
        RegistrationStatus status = RegistrationStatus.Confirmed)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var registration = new Registration
        {
            EventId = eventId,
            UserId = userId,
            Status = status,
            RegisteredAt = DateTime.UtcNow
        };

        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        return registration.Id;
    }

    #endregion

    #region Skill Helpers

    /// <summary>
    /// Creates a skill and returns its ID.
    /// </summary>
    public static async Task<int> CreateSkillAsync(
        IServiceProvider services,
        string name = "Test Skill",
        string category = "General")
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var skill = new Skill
        {
            Name = name,
            Description = category,
            CreatedAt = DateTime.UtcNow
        };

        context.Skills.Add(skill);
        await context.SaveChangesAsync();

        return skill.Id;
    }

    /// <summary>
    /// Assigns a skill to a user.
    /// </summary>
    public static async Task AssignSkillToUserAsync(
        IServiceProvider services,
        int userId,
        int skillId)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var userSkill = new UserSkill
        {
            UserId = userId,
            SkillId = skillId,
            AddedAt = DateTime.UtcNow
        };

        context.Set<UserSkill>().Add(userSkill);
        await context.SaveChangesAsync();
    }

    #endregion

    #region Assertions

    /// <summary>
    /// Deserializes JSON response content to specified type.
    /// </summary>
    public static async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    #endregion
}
