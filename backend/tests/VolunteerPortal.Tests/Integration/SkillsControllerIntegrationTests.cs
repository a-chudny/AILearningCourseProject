using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.DTOs;
using VolunteerPortal.API.Models.DTOs.Auth;
using VolunteerPortal.API.Models.DTOs.Skills;
using VolunteerPortal.API.Models.Entities;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.Tests.Integration;

/// <summary>
/// Integration tests for Skills endpoints.
/// Tests skill retrieval and user skill management.
/// </summary>
public class SkillsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SkillsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.EnsureDatabaseCreated();
        _client = _factory.CreateClient();
    }

    #region Get All Skills Tests

    [Fact]
    public async Task GetAllSkills_Public_ReturnsOk()
    {
        // Arrange
        await SeedSkillsAsync();

        // Act - No auth needed for public endpoint
        var response = await _client.GetAsync("/api/skills");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var skills = JsonSerializer.Deserialize<List<SkillResponse>>(content, JsonOptions);

        Assert.NotNull(skills);
        Assert.NotEmpty(skills);
    }

    [Fact]
    public async Task GetAllSkills_ReturnsSkillsWithCategories()
    {
        // Arrange
        await SeedSkillsAsync();

        // Act
        var response = await _client.GetAsync("/api/skills");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var skills = JsonSerializer.Deserialize<List<SkillResponse>>(content, JsonOptions);

        Assert.NotNull(skills);
        Assert.All(skills, skill =>
        {
            Assert.NotEmpty(skill.Name);
            Assert.NotEmpty(skill.Category);
        });
    }

    #endregion

    #region Get My Skills Tests

    [Fact]
    public async Task GetMySkills_Authenticated_ReturnsOk()
    {
        // Arrange
        var (userId, skillId) = await SeedUserWithSkillAsync();
        var token = await GetVolunteerTokenByIdAsync(userId);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/skills/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var skills = JsonSerializer.Deserialize<List<SkillResponse>>(content, JsonOptions);

        Assert.NotNull(skills);
        Assert.Contains(skills, s => s.Id == skillId);
    }

    [Fact]
    public async Task GetMySkills_NoSkills_ReturnsEmptyList()
    {
        // Arrange
        var token = await GetVolunteerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/skills/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var skills = JsonSerializer.Deserialize<List<SkillResponse>>(content, JsonOptions);

        Assert.NotNull(skills);
        Assert.Empty(skills);
    }

    [Fact]
    public async Task GetMySkills_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync("/api/skills/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Update My Skills Tests

    [Fact]
    public async Task UpdateMySkills_AddSkills_ReturnsNoContent()
    {
        // Arrange
        var skillIds = await SeedMultipleSkillsAsync(3);
        var token = await GetVolunteerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new UpdateUserSkillsRequest { SkillIds = skillIds };

        // Act
        var response = await _client.PutAsJsonAsync("/api/skills/me", request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify skills were added
        var getResponse = await _client.GetAsync("/api/skills/me");
        var content = await getResponse.Content.ReadAsStringAsync();
        var skills = JsonSerializer.Deserialize<List<SkillResponse>>(content, JsonOptions);

        Assert.NotNull(skills);
        Assert.Equal(skillIds.Count, skills.Count);
    }

    [Fact]
    public async Task UpdateMySkills_ReplaceSkills_ReturnsNoContent()
    {
        // Arrange
        var initialSkillIds = await SeedMultipleSkillsAsync(2);
        var newSkillIds = await SeedMultipleSkillsAsync(3);
        var token = await GetVolunteerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Add initial skills
        var initialRequest = new UpdateUserSkillsRequest { SkillIds = initialSkillIds };
        await _client.PutAsJsonAsync("/api/skills/me", initialRequest);

        // Act - Replace with new skills
        var newRequest = new UpdateUserSkillsRequest { SkillIds = newSkillIds };
        var response = await _client.PutAsJsonAsync("/api/skills/me", newRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify only new skills remain
        var getResponse = await _client.GetAsync("/api/skills/me");
        var content = await getResponse.Content.ReadAsStringAsync();
        var skills = JsonSerializer.Deserialize<List<SkillResponse>>(content, JsonOptions);

        Assert.NotNull(skills);
        Assert.Equal(newSkillIds.Count, skills.Count);
        Assert.All(skills, s => Assert.Contains(s.Id, newSkillIds));
    }

    [Fact]
    public async Task UpdateMySkills_ClearAll_ReturnsNoContent()
    {
        // Arrange
        var skillIds = await SeedMultipleSkillsAsync(2);
        var token = await GetVolunteerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Add skills first
        var initialRequest = new UpdateUserSkillsRequest { SkillIds = skillIds };
        await _client.PutAsJsonAsync("/api/skills/me", initialRequest);

        // Act - Clear all skills
        var clearRequest = new UpdateUserSkillsRequest { SkillIds = new List<int>() };
        var response = await _client.PutAsJsonAsync("/api/skills/me", clearRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify skills are cleared
        var getResponse = await _client.GetAsync("/api/skills/me");
        var content = await getResponse.Content.ReadAsStringAsync();
        var skills = JsonSerializer.Deserialize<List<SkillResponse>>(content, JsonOptions);

        Assert.NotNull(skills);
        Assert.Empty(skills);
    }

    [Fact]
    public async Task UpdateMySkills_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;
        var request = new UpdateUserSkillsRequest { SkillIds = new List<int> { 1 } };

        // Act
        var response = await _client.PutAsJsonAsync("/api/skills/me", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Complete Skill Flow Tests

    [Fact]
    public async Task CompleteFlow_ViewSkillsAssignAndVerify()
    {
        // Arrange
        await SeedSkillsAsync();
        var token = await GetVolunteerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act 1 - Get all available skills
        var allSkillsResponse = await _client.GetAsync("/api/skills");
        Assert.Equal(HttpStatusCode.OK, allSkillsResponse.StatusCode);
        var allSkillsContent = await allSkillsResponse.Content.ReadAsStringAsync();
        var allSkills = JsonSerializer.Deserialize<List<SkillResponse>>(allSkillsContent, JsonOptions);
        Assert.NotNull(allSkills);
        Assert.NotEmpty(allSkills);

        // Act 2 - Select some skills
        var selectedSkillIds = allSkills.Take(3).Select(s => s.Id).ToList();
        var updateRequest = new UpdateUserSkillsRequest { SkillIds = selectedSkillIds };
        var updateResponse = await _client.PutAsJsonAsync("/api/skills/me", updateRequest);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // Act 3 - Verify my skills
        var mySkillsResponse = await _client.GetAsync("/api/skills/me");
        Assert.Equal(HttpStatusCode.OK, mySkillsResponse.StatusCode);
        var mySkillsContent = await mySkillsResponse.Content.ReadAsStringAsync();
        var mySkills = JsonSerializer.Deserialize<List<SkillResponse>>(mySkillsContent, JsonOptions);

        // Assert
        Assert.NotNull(mySkills);
        Assert.Equal(selectedSkillIds.Count, mySkills.Count);
        Assert.All(mySkills, s => Assert.Contains(s.Id, selectedSkillIds));
    }

    #endregion

    #region Helper Methods

    private async Task SeedSkillsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Check if skills already exist
        if (context.Skills.Any())
            return;

        var skills = new List<Skill>
        {
            new() { Name = "First Aid", Description = "Medical", CreatedAt = DateTime.UtcNow },
            new() { Name = "Event Planning", Description = "Management", CreatedAt = DateTime.UtcNow },
            new() { Name = "Communication", Description = "Soft Skills", CreatedAt = DateTime.UtcNow },
            new() { Name = "Data Entry", Description = "Office", CreatedAt = DateTime.UtcNow },
            new() { Name = "Teaching", Description = "Education", CreatedAt = DateTime.UtcNow }
        };

        context.Skills.AddRange(skills);
        await context.SaveChangesAsync();
    }

    private async Task<List<int>> SeedMultipleSkillsAsync(int count)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var skills = new List<Skill>();
        for (int i = 0; i < count; i++)
        {
            skills.Add(new Skill
            {
                Name = $"Skill {Guid.NewGuid()}",
                Description = "Test Category",
                CreatedAt = DateTime.UtcNow
            });
        }

        context.Skills.AddRange(skills);
        await context.SaveChangesAsync();

        return skills.Select(s => s.Id).ToList();
    }

    private async Task<(int userId, int skillId)> SeedUserWithSkillAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var email = $"volunteer{Guid.NewGuid()}@test.com";
        var volunteer = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Name = "Volunteer With Skill",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.Add(volunteer);
        await context.SaveChangesAsync();

        var skill = new Skill
        {
            Name = $"Skill {Guid.NewGuid()}",
            Description = "Test Category",
            CreatedAt = DateTime.UtcNow
        };

        context.Skills.Add(skill);
        await context.SaveChangesAsync();

        var userSkill = new UserSkill
        {
            UserId = volunteer.Id,
            SkillId = skill.Id,
            AddedAt = DateTime.UtcNow
        };

        context.Set<UserSkill>().Add(userSkill);
        await context.SaveChangesAsync();

        return (volunteer.Id, skill.Id);
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

    private async Task<string> GetVolunteerTokenByIdAsync(int userId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"User {userId} not found");

        // We need to login, but we don't have the password
        // For this test, we'll create a new user and use that
        var email = $"volunteer{Guid.NewGuid()}@test.com";
        var password = "Password123";

        // Update the existing user's email and password
        user.Email = email;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
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
