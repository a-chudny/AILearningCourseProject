using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.DTOs.Admin;
using VolunteerPortal.API.Models.DTOs.Auth;
using VolunteerPortal.API.Models.Entities;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.Tests.Integration;

/// <summary>
/// Integration tests for Admin endpoints.
/// Tests admin dashboard stats, user management, role changes, and soft delete operations.
/// </summary>
public class AdminControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AdminControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.EnsureDatabaseCreated();
        _client = _factory.CreateClient();
    }

    #region Admin Stats Tests

    [Fact]
    public async Task GetStats_AsAdmin_ReturnsOk()
    {
        // Arrange
        await SeedTestDataAsync();
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/admin/stats");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var stats = JsonSerializer.Deserialize<AdminStatsResponse>(content, JsonOptions);

        Assert.NotNull(stats);
        Assert.True(stats.TotalUsers >= 0);
        Assert.True(stats.TotalEvents >= 0);
        Assert.True(stats.TotalRegistrations >= 0);
    }

    [Fact]
    public async Task GetStats_AsOrganizer_ReturnsForbidden()
    {
        // Arrange
        var token = await GetOrganizerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/admin/stats");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetStats_AsVolunteer_ReturnsForbidden()
    {
        // Arrange
        var token = await GetVolunteerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/admin/stats");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetStats_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync("/api/admin/stats");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Admin User List Tests

    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsOk()
    {
        // Arrange
        await SeedTestDataAsync();
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var userList = JsonSerializer.Deserialize<AdminUserListResponse>(content, JsonOptions);

        Assert.NotNull(userList);
        Assert.NotNull(userList.Users);
        Assert.True(userList.TotalCount > 0);
    }

    [Fact]
    public async Task GetUsers_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await SeedMultipleUsersAsync(15);
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/admin/users?page=1&pageSize=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var userList = JsonSerializer.Deserialize<AdminUserListResponse>(content, JsonOptions);

        Assert.NotNull(userList);
        Assert.Equal(1, userList.Page);
        Assert.Equal(5, userList.PageSize);
        Assert.True(userList.Users.Count <= 5);
    }

    [Fact]
    public async Task GetUsers_WithSearch_ReturnsFilteredResults()
    {
        // Arrange
        await SeedUserWithNameAsync("Unique Test Name");
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/admin/users?search=Unique");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var userList = JsonSerializer.Deserialize<AdminUserListResponse>(content, JsonOptions);

        Assert.NotNull(userList);
        Assert.Contains(userList.Users, u => u.Name.Contains("Unique"));
    }

    [Fact]
    public async Task GetUsers_IncludeDeleted_ReturnsDeletedUsers()
    {
        // Arrange
        var deletedUserId = await SeedDeletedUserAsync();
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/admin/users?includeDeleted=true");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var userList = JsonSerializer.Deserialize<AdminUserListResponse>(content, JsonOptions);

        Assert.NotNull(userList);
        Assert.Contains(userList.Users, u => u.Id == deletedUserId && u.IsDeleted);
    }

    [Fact]
    public async Task GetUsers_AsOrganizer_ReturnsForbidden()
    {
        // Arrange
        var token = await GetOrganizerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Update User Role Tests

    [Fact]
    public async Task UpdateUserRole_AsAdmin_ReturnsOk()
    {
        // Arrange
        var volunteerId = await SeedVolunteerUserAsync();
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new UpdateUserRoleRequest { Role = (int)UserRole.Organizer };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/admin/users/{volunteerId}/role", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var updatedUser = JsonSerializer.Deserialize<AdminUserResponse>(content, JsonOptions);

        Assert.NotNull(updatedUser);
        Assert.Equal((int)UserRole.Organizer, updatedUser.Role);
    }

    [Fact]
    public async Task UpdateUserRole_OwnRole_ReturnsBadRequest()
    {
        // Arrange
        var (adminId, token) = await GetAdminWithIdAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new UpdateUserRoleRequest { Role = (int)UserRole.Volunteer };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/admin/users/{adminId}/role", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUserRole_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new UpdateUserRoleRequest { Role = (int)UserRole.Organizer };

        // Act
        var response = await _client.PutAsJsonAsync("/api/admin/users/99999/role", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUserRole_DeletedUser_ReturnsBadRequest()
    {
        // Arrange
        var deletedUserId = await SeedDeletedUserAsync();
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new UpdateUserRoleRequest { Role = (int)UserRole.Organizer };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/admin/users/{deletedUserId}/role", request);

        // Assert - InvalidOperationException (without "already" in message) maps to 400 BadRequest
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUserRole_AsOrganizer_ReturnsForbidden()
    {
        // Arrange
        var volunteerId = await SeedVolunteerUserAsync();
        var token = await GetOrganizerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new UpdateUserRoleRequest { Role = (int)UserRole.Organizer };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/admin/users/{volunteerId}/role", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Soft Delete User Tests

    [Fact]
    public async Task DeleteUser_AsAdmin_ReturnsOk()
    {
        // Arrange
        var volunteerId = await SeedVolunteerUserAsync();
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync($"/api/admin/users/{volunteerId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify user is soft-deleted
        var checkResponse = await _client.GetAsync("/api/admin/users?includeDeleted=true");
        var content = await checkResponse.Content.ReadAsStringAsync();
        var userList = JsonSerializer.Deserialize<AdminUserListResponse>(content, JsonOptions);

        Assert.Contains(userList!.Users, u => u.Id == volunteerId && u.IsDeleted);
    }

    [Fact]
    public async Task DeleteUser_OwnAccount_ReturnsBadRequest()
    {
        // Arrange
        var (adminId, token) = await GetAdminWithIdAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync($"/api/admin/users/{adminId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync("/api/admin/users/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_AlreadyDeleted_ReturnsConflict()
    {
        // Arrange
        var deletedUserId = await SeedDeletedUserAsync();
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync($"/api/admin/users/{deletedUserId}");

        // Assert - InvalidOperationException maps to 409 Conflict
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_AsOrganizer_ReturnsForbidden()
    {
        // Arrange
        var volunteerId = await SeedVolunteerUserAsync();
        var token = await GetOrganizerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync($"/api/admin/users/{volunteerId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Complete Admin Flow Tests

    [Fact]
    public async Task CompleteFlow_ViewStatsListUsersChangeRoleDeleteUser()
    {
        // Arrange
        await SeedTestDataAsync();
        var volunteerId = await SeedVolunteerUserAsync();
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act 1 - Get stats
        var statsResponse = await _client.GetAsync("/api/admin/stats");
        Assert.Equal(HttpStatusCode.OK, statsResponse.StatusCode);

        // Act 2 - List users
        var usersResponse = await _client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);

        // Act 3 - Update user role
        var roleRequest = new UpdateUserRoleRequest { Role = (int)UserRole.Organizer };
        var roleResponse = await _client.PutAsJsonAsync($"/api/admin/users/{volunteerId}/role", roleRequest);
        Assert.Equal(HttpStatusCode.OK, roleResponse.StatusCode);

        // Act 4 - Delete user
        var deleteResponse = await _client.DeleteAsync($"/api/admin/users/{volunteerId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Assert - User should be deleted
        var finalUsersResponse = await _client.GetAsync("/api/admin/users?includeDeleted=true");
        var content = await finalUsersResponse.Content.ReadAsStringAsync();
        var userList = JsonSerializer.Deserialize<AdminUserListResponse>(content, JsonOptions);

        var deletedUser = userList!.Users.FirstOrDefault(u => u.Id == volunteerId);
        Assert.NotNull(deletedUser);
        Assert.True(deletedUser.IsDeleted);
        Assert.Equal((int)UserRole.Organizer, deletedUser.Role); // Role change persisted
    }

    #endregion

    #region Helper Methods

    private async Task SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Seed some test data
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
    }

    private async Task SeedMultipleUsersAsync(int count)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        for (int i = 0; i < count; i++)
        {
            var user = new User
            {
                Email = $"user{Guid.NewGuid()}@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                Name = $"Test User {i}",
                Role = UserRole.Volunteer,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Users.Add(user);
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedUserWithNameAsync(string name)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User
        {
            Email = $"user{Guid.NewGuid()}@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Name = name,
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    private async Task<int> SeedDeletedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User
        {
            Email = $"deleted{Guid.NewGuid()}@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Name = "Deleted User",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = true // Already deleted
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.Id;
    }

    private async Task<int> SeedVolunteerUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User
        {
            Email = $"volunteer{Guid.NewGuid()}@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Name = "Target Volunteer",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.Id;
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

    private async Task<(int adminId, string token)> GetAdminWithIdAsync()
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

        var token = await LoginAndGetTokenAsync(email, password);
        return (admin.Id, token);
    }

    private async Task<string> GetOrganizerTokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var email = $"organizer{Guid.NewGuid()}@test.com";
        var password = "Password123";

        var organizer = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Name = "Test Organizer",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.Add(organizer);
        await context.SaveChangesAsync();

        return await LoginAndGetTokenAsync(email, password);
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
