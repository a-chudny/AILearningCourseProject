using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.Entities;
using VolunteerPortal.API.Models.Enums;
using VolunteerPortal.API.Services;

namespace VolunteerPortal.Tests.Services;

public class SkillServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly SkillService _service;

    public SkillServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new SkillService(_context);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var skills = new List<Skill>
        {
            new() { Id = 1, Name = "First Aid / CPR", Description = "Medical", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Teaching / Tutoring", Description = "Education", CreatedAt = DateTime.UtcNow },
            new() { Id = 3, Name = "Driving", Description = "Transportation", CreatedAt = DateTime.UtcNow },
            new() { Id = 4, Name = "Cooking", Description = "Food Service", CreatedAt = DateTime.UtcNow },
            new() { Id = 5, Name = "IT Support", Description = "Technology", CreatedAt = DateTime.UtcNow }
        };

        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            Name = "Test User",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow
        };

        _context.Skills.AddRange(skills);
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllSkillsAsync_ReturnsAllSkillsOrderedByName()
    {
        // Act
        var result = await _service.GetAllSkillsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.First().Name.Should().Be("Cooking"); // Alphabetically first
        result.Last().Name.Should().Be("Teaching / Tutoring"); // Alphabetically last
    }

    [Fact]
    public async Task GetAllSkillsAsync_MapsDescriptionToCategory()
    {
        // Act
        var result = await _service.GetAllSkillsAsync();

        // Assert
        var firstAidSkill = result.First(s => s.Name == "First Aid / CPR");
        firstAidSkill.Category.Should().Be("Medical");
    }

    [Fact]
    public async Task GetUserSkillsAsync_ReturnsEmptyListWhenUserHasNoSkills()
    {
        // Act
        var result = await _service.GetUserSkillsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserSkillsAsync_ReturnsUserSkillsOrderedByName()
    {
        // Arrange
        var userSkills = new List<UserSkill>
        {
            new() { UserId = 1, SkillId = 5 }, // IT Support
            new() { UserId = 1, SkillId = 1 }, // First Aid / CPR
            new() { UserId = 1, SkillId = 3 }  // Driving
        };
        await _context.UserSkills.AddRangeAsync(userSkills);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUserSkillsAsync(1);

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Driving");
        result[1].Name.Should().Be("First Aid / CPR");
        result[2].Name.Should().Be("IT Support");
    }

    [Fact]
    public async Task UpdateUserSkillsAsync_AddsSkillsWhenUserHasNone()
    {
        // Arrange
        var skillIds = new List<int> { 1, 2, 3 };

        // Act
        await _service.UpdateUserSkillsAsync(1, skillIds);

        // Assert
        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == 1)
            .ToListAsync();

        userSkills.Should().HaveCount(3);
        userSkills.Select(us => us.SkillId).Should().BeEquivalentTo(skillIds);
    }

    [Fact]
    public async Task UpdateUserSkillsAsync_ReplacesExistingSkills()
    {
        // Arrange
        var initialSkills = new List<UserSkill>
        {
            new() { UserId = 1, SkillId = 1 },
            new() { UserId = 1, SkillId = 2 }
        };
        await _context.UserSkills.AddRangeAsync(initialSkills);
        await _context.SaveChangesAsync();

        var newSkillIds = new List<int> { 3, 4, 5 };

        // Act
        await _service.UpdateUserSkillsAsync(1, newSkillIds);

        // Assert
        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == 1)
            .ToListAsync();

        userSkills.Should().HaveCount(3);
        userSkills.Select(us => us.SkillId).Should().BeEquivalentTo(newSkillIds);
        userSkills.Should().NotContain(us => us.SkillId == 1);
        userSkills.Should().NotContain(us => us.SkillId == 2);
    }

    [Fact]
    public async Task UpdateUserSkillsAsync_AllowsEmptySkillList()
    {
        // Arrange
        var initialSkills = new List<UserSkill>
        {
            new() { UserId = 1, SkillId = 1 },
            new() { UserId = 1, SkillId = 2 }
        };
        await _context.UserSkills.AddRangeAsync(initialSkills);
        await _context.SaveChangesAsync();

        var emptySkillIds = new List<int>();

        // Act
        await _service.UpdateUserSkillsAsync(1, emptySkillIds);

        // Assert
        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == 1)
            .ToListAsync();

        userSkills.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateUserSkillsAsync_ThrowsArgumentExceptionForInvalidSkillId()
    {
        // Arrange
        var invalidSkillIds = new List<int> { 1, 999 }; // 999 doesn't exist

        // Act
        var act = async () => await _service.UpdateUserSkillsAsync(1, invalidSkillIds);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid skill IDs: 999");
    }

    [Fact]
    public async Task UpdateUserSkillsAsync_ThrowsArgumentExceptionForMultipleInvalidSkillIds()
    {
        // Arrange
        var invalidSkillIds = new List<int> { 1, 999, 888, 3 }; // 999 and 888 don't exist

        // Act
        var act = async () => await _service.UpdateUserSkillsAsync(1, invalidSkillIds);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid skill IDs: 999, 888");
    }

    [Fact]
    public async Task UpdateUserSkillsAsync_AllowsDuplicateSkillIds()
    {
        // Arrange
        var skillIdsWithDuplicates = new List<int> { 1, 2, 2, 3 };

        // Act
        await _service.UpdateUserSkillsAsync(1, skillIdsWithDuplicates);

        // Assert
        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == 1)
            .ToListAsync();

        // Should handle duplicates gracefully (likely creates multiple records)
        userSkills.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
