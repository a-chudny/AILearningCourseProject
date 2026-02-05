using Microsoft.EntityFrameworkCore;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.Entities;
using VolunteerPortal.API.Models.Enums;
using VolunteerPortal.API.Services;
using Xunit;

namespace VolunteerPortal.Tests.Services;

/// <summary>
/// Unit tests for ReportService
/// </summary>
public class ReportServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ReportService _sut;

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _sut = new ReportService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetUsersForExportAsync Tests

    [Fact]
    public async Task GetUsersForExportAsync_ReturnsActiveUsersOnly()
    {
        // Arrange
        var activeUser = new User
        {
            Id = 1,
            Email = "active@example.com",
            Name = "Active User",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var deletedUser = new User
        {
            Id = 2,
            Email = "deleted@example.com",
            Name = "Deleted User",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddRangeAsync(activeUser, deletedUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetUsersForExportAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("active@example.com", result.First().Email);
    }

    [Fact]
    public async Task GetUsersForExportAsync_IncludesUserSkillsAsCommaSeparatedString()
    {
        // Arrange
        var skill1 = new Skill { Id = 1, Name = "First Aid", Description = "Medical skills" };
        var skill2 = new Skill { Id = 2, Name = "Cooking", Description = "Food prep skills" };
        await _context.Skills.AddRangeAsync(skill1, skill2);

        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            Name = "Test User",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var userSkill1 = new UserSkill { UserId = 1, SkillId = 1 };
        var userSkill2 = new UserSkill { UserId = 1, SkillId = 2 };
        await _context.UserSkills.AddRangeAsync(userSkill1, userSkill2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetUsersForExportAsync();

        // Assert
        Assert.Single(result);
        var userDto = result.First();
        Assert.Contains("First Aid", userDto.Skills);
        Assert.Contains("Cooking", userDto.Skills);
    }

    [Fact]
    public async Task GetUsersForExportAsync_ReturnsCorrectRoleAsString()
    {
        // Arrange
        var adminUser = new User
        {
            Id = 1,
            Email = "admin@example.com",
            Name = "Admin User",
            PasswordHash = "hash",
            Role = UserRole.Admin,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(adminUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetUsersForExportAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Admin", result.First().Role);
    }

    #endregion

    #region GetEventsForExportAsync Tests

    [Fact]
    public async Task GetEventsForExportAsync_ReturnsActiveEventsOnly()
    {
        // Arrange
        var organizer = new User
        {
            Id = 1,
            Email = "organizer@example.com",
            Name = "Event Organizer",
            PasswordHash = "hash",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(organizer);

        var activeEvent = new Event
        {
            Id = 1,
            Title = "Active Event",
            Description = "Test",
            StartTime = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 120,
            Location = "Test Location",
            Capacity = 10,
            OrganizerId = 1,
            Status = EventStatus.Active,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var deletedEvent = new Event
        {
            Id = 2,
            Title = "Deleted Event",
            Description = "Test",
            StartTime = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 120,
            Location = "Test Location",
            Capacity = 10,
            OrganizerId = 1,
            Status = EventStatus.Active,
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Events.AddRangeAsync(activeEvent, deletedEvent);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetEventsForExportAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Active Event", result.First().Title);
    }

    [Fact]
    public async Task GetEventsForExportAsync_CountsOnlyConfirmedRegistrations()
    {
        // Arrange
        var organizer = new User
        {
            Id = 1,
            Email = "organizer@example.com",
            Name = "Event Organizer",
            PasswordHash = "hash",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow
        };
        var volunteer1 = new User
        {
            Id = 2,
            Email = "vol1@example.com",
            Name = "Vol One",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow
        };
        var volunteer2 = new User
        {
            Id = 3,
            Email = "vol2@example.com",
            Name = "Vol Two",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddRangeAsync(organizer, volunteer1, volunteer2);

        var evt = new Event
        {
            Id = 1,
            Title = "Test Event",
            Description = "Test",
            StartTime = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 120,
            Location = "Test Location",
            Capacity = 10,
            OrganizerId = 1,
            Status = EventStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Events.AddAsync(evt);
        await _context.SaveChangesAsync();

        var confirmedReg = new Registration
        {
            EventId = 1,
            UserId = 2,
            Status = RegistrationStatus.Confirmed,
            RegisteredAt = DateTime.UtcNow
        };
        var cancelledReg = new Registration
        {
            EventId = 1,
            UserId = 3,
            Status = RegistrationStatus.Cancelled,
            RegisteredAt = DateTime.UtcNow
        };
        await _context.Registrations.AddRangeAsync(confirmedReg, cancelledReg);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetEventsForExportAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result.First().RegistrationCount);
    }

    [Fact]
    public async Task GetEventsForExportAsync_IncludesOrganizerDetails()
    {
        // Arrange
        var organizer = new User
        {
            Id = 1,
            Email = "organizer@example.com",
            Name = "John Smith",
            PasswordHash = "hash",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(organizer);

        var evt = new Event
        {
            Id = 1,
            Title = "Test Event",
            Description = "Test",
            StartTime = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 120,
            Location = "Test Location",
            Capacity = 10,
            OrganizerId = 1,
            Status = EventStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Events.AddAsync(evt);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetEventsForExportAsync();

        // Assert
        Assert.Single(result);
        var eventDto = result.First();
        Assert.Equal("John Smith", eventDto.OrganizerName);
        Assert.Equal("organizer@example.com", eventDto.OrganizerEmail);
    }

    #endregion

    #region GetRegistrationsForExportAsync Tests

    [Fact]
    public async Task GetRegistrationsForExportAsync_ReturnsOnlyConfirmedRegistrations()
    {
        // Arrange
        var organizer = new User
        {
            Id = 1,
            Email = "organizer@example.com",
            Name = "Event Organizer",
            PasswordHash = "hash",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow
        };
        var volunteer = new User
        {
            Id = 2,
            Email = "vol@example.com",
            Name = "Vol User",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddRangeAsync(organizer, volunteer);

        var evt = new Event
        {
            Id = 1,
            Title = "Test Event",
            Description = "Test",
            StartTime = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 120,
            Location = "Test Location",
            Capacity = 10,
            OrganizerId = 1,
            Status = EventStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Events.AddAsync(evt);
        await _context.SaveChangesAsync();

        var confirmedReg = new Registration
        {
            Id = 1,
            EventId = 1,
            UserId = 2,
            Status = RegistrationStatus.Confirmed,
            RegisteredAt = DateTime.UtcNow
        };
        var cancelledReg = new Registration
        {
            Id = 2,
            EventId = 1,
            UserId = 2,
            Status = RegistrationStatus.Cancelled,
            RegisteredAt = DateTime.UtcNow
        };
        await _context.Registrations.AddRangeAsync(confirmedReg, cancelledReg);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetRegistrationsForExportAsync(null, null);

        // Assert
        Assert.Single(result);
        Assert.Equal("Confirmed", result.First().Status);
    }

    [Fact]
    public async Task GetRegistrationsForExportAsync_FiltersDeletedEventsAndUsers()
    {
        // Arrange
        var organizer = new User
        {
            Id = 1,
            Email = "organizer@example.com",
            Name = "Event Organizer",
            PasswordHash = "hash",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow
        };
        var activeVol = new User
        {
            Id = 2,
            Email = "active@example.com",
            Name = "Active Vol",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var deletedVol = new User
        {
            Id = 3,
            Email = "deleted@example.com",
            Name = "Deleted Vol",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddRangeAsync(organizer, activeVol, deletedVol);

        var evt = new Event
        {
            Id = 1,
            Title = "Test Event",
            Description = "Test",
            StartTime = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 120,
            Location = "Test Location",
            Capacity = 10,
            OrganizerId = 1,
            Status = EventStatus.Active,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Events.AddAsync(evt);
        await _context.SaveChangesAsync();

        var activeReg = new Registration
        {
            Id = 1,
            EventId = 1,
            UserId = 2,
            Status = RegistrationStatus.Confirmed,
            RegisteredAt = DateTime.UtcNow
        };
        var deletedUserReg = new Registration
        {
            Id = 2,
            EventId = 1,
            UserId = 3,
            Status = RegistrationStatus.Confirmed,
            RegisteredAt = DateTime.UtcNow
        };
        await _context.Registrations.AddRangeAsync(activeReg, deletedUserReg);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetRegistrationsForExportAsync(null, null);

        // Assert
        Assert.Single(result);
        Assert.Equal("active@example.com", result.First().VolunteerEmail);
    }

    [Fact]
    public async Task GetRegistrationsForExportAsync_AppliesDateFilterOnRegisteredAt()
    {
        // Arrange
        var organizer = new User
        {
            Id = 1,
            Email = "organizer@example.com",
            Name = "Event Organizer",
            PasswordHash = "hash",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow
        };
        var volunteer = new User
        {
            Id = 2,
            Email = "vol@example.com",
            Name = "Vol User",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddRangeAsync(organizer, volunteer);

        var evt = new Event
        {
            Id = 1,
            Title = "Test Event",
            Description = "Test",
            StartTime = DateTime.UtcNow.AddDays(30),
            DurationMinutes = 120,
            Location = "Test Location",
            Capacity = 10,
            OrganizerId = 1,
            Status = EventStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Events.AddAsync(evt);
        await _context.SaveChangesAsync();

        var inRangeReg = new Registration
        {
            Id = 1,
            EventId = 1,
            UserId = 2,
            Status = RegistrationStatus.Confirmed,
            RegisteredAt = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc)
        };
        var outOfRangeReg = new Registration
        {
            Id = 2,
            EventId = 1,
            UserId = 2,
            Status = RegistrationStatus.Confirmed,
            RegisteredAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        await _context.Registrations.AddRangeAsync(inRangeReg, outOfRangeReg);
        await _context.SaveChangesAsync();

        // Act
        var startDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2024, 6, 30, 0, 0, 0, DateTimeKind.Utc);
        var result = await _sut.GetRegistrationsForExportAsync(startDate, endDate);

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc), result.First().RegisteredAt);
    }

    #endregion

    #region GetSkillsSummaryForExportAsync Tests

    [Fact]
    public async Task GetSkillsSummaryForExportAsync_CountsOnlyActiveUsersWithSkill()
    {
        // Arrange
        var skill = new Skill { Id = 1, Name = "First Aid", Description = "Medical skills" };
        await _context.Skills.AddAsync(skill);

        var activeUser = new User
        {
            Id = 1,
            Email = "active@example.com",
            Name = "Active User",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var deletedUser = new User
        {
            Id = 2,
            Email = "deleted@example.com",
            Name = "Deleted User",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddRangeAsync(activeUser, deletedUser);
        await _context.SaveChangesAsync();

        var activeUserSkill = new UserSkill { UserId = 1, SkillId = 1 };
        var deletedUserSkill = new UserSkill { UserId = 2, SkillId = 1 };
        await _context.UserSkills.AddRangeAsync(activeUserSkill, deletedUserSkill);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetSkillsSummaryForExportAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result.First().VolunteerCount);
    }

    [Fact]
    public async Task GetSkillsSummaryForExportAsync_CountsOnlyActiveEventsWithSkill()
    {
        // Arrange
        var skill = new Skill { Id = 1, Name = "First Aid", Description = "Medical skills" };
        await _context.Skills.AddAsync(skill);

        var organizer = new User
        {
            Id = 1,
            Email = "organizer@example.com",
            Name = "Event Organizer",
            PasswordHash = "hash",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(organizer);

        var activeEvent = new Event
        {
            Id = 1,
            Title = "Active Event",
            Description = "Test",
            StartTime = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 120,
            Location = "Test Location",
            Capacity = 10,
            OrganizerId = 1,
            Status = EventStatus.Active,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var deletedEvent = new Event
        {
            Id = 2,
            Title = "Deleted Event",
            Description = "Test",
            StartTime = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 120,
            Location = "Test Location",
            Capacity = 10,
            OrganizerId = 1,
            Status = EventStatus.Active,
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Events.AddRangeAsync(activeEvent, deletedEvent);
        await _context.SaveChangesAsync();

        var activeEventSkill = new EventSkill { EventId = 1, SkillId = 1 };
        var deletedEventSkill = new EventSkill { EventId = 2, SkillId = 1 };
        await _context.EventSkills.AddRangeAsync(activeEventSkill, deletedEventSkill);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetSkillsSummaryForExportAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result.First().EventCount);
    }

    [Fact]
    public async Task GetSkillsSummaryForExportAsync_IncludesSkillNameAndDescription()
    {
        // Arrange
        var skill = new Skill { Id = 1, Name = "First Aid", Description = "Medical and emergency response skills" };
        await _context.Skills.AddAsync(skill);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetSkillsSummaryForExportAsync();

        // Assert
        Assert.Single(result);
        var skillDto = result.First();
        Assert.Equal("First Aid", skillDto.SkillName);
        Assert.Equal("Medical and emergency response skills", skillDto.Description);
    }

    #endregion
}
