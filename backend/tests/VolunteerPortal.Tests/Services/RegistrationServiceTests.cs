using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.Entities;
using VolunteerPortal.API.Models.Enums;
using VolunteerPortal.API.Services;

namespace VolunteerPortal.Tests.Services;

public class RegistrationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly RegistrationService _service;

    public RegistrationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new RegistrationService(_context);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var user1 = new User
        {
            Id = 1,
            Email = "volunteer@test.com",
            Name = "Test Volunteer",
            PasswordHash = "hash",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new User
        {
            Id = 2,
            Email = "organizer@test.com",
            Name = "Test Organizer",
            PasswordHash = "hash",
            Role = UserRole.Organizer,
            CreatedAt = DateTime.UtcNow
        };

        var futureEvent = new Event
        {
            Id = 1,
            Title = "Future Event",
            Description = "Test Event",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 10,
            Status = EventStatus.Active,
            OrganizerId = 2,
            CreatedAt = DateTime.UtcNow
        };

        var pastEvent = new Event
        {
            Id = 2,
            Title = "Past Event",
            Description = "Test Past Event",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(-7),
            DurationMinutes = 120,
            Capacity = 10,
            Status = EventStatus.Active,
            OrganizerId = 2,
            CreatedAt = DateTime.UtcNow
        };

        var cancelledEvent = new Event
        {
            Id = 3,
            Title = "Cancelled Event",
            Description = "Test Cancelled Event",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 10,
            Status = EventStatus.Cancelled,
            OrganizerId = 2,
            CreatedAt = DateTime.UtcNow
        };

        var fullEvent = new Event
        {
            Id = 4,
            Title = "Full Event",
            Description = "Test Full Event",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 1,
            Status = EventStatus.Active,
            OrganizerId = 2,
            CreatedAt = DateTime.UtcNow
        };

        var eventWithDeadline = new Event
        {
            Id = 5,
            Title = "Event With Deadline",
            Description = "Test Event With Past Deadline",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 10,
            Status = EventStatus.Active,
            RegistrationDeadline = DateTime.UtcNow.AddDays(-1),
            OrganizerId = 2,
            CreatedAt = DateTime.UtcNow
        };

        var conflictingEvent = new Event
        {
            Id = 6,
            Title = "Conflicting Time Event",
            Description = "Event at same time",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7).AddHours(1),
            DurationMinutes = 120,
            Capacity = 10,
            Status = EventStatus.Active,
            OrganizerId = 2,
            CreatedAt = DateTime.UtcNow
        };

        var nonConflictingEvent = new Event
        {
            Id = 7,
            Title = "Non-Conflicting Event",
            Description = "Event at different time",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(14), // Different week
            DurationMinutes = 120,
            Capacity = 10,
            Status = EventStatus.Active,
            OrganizerId = 2,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.AddRange(user1, user2);
        _context.Events.AddRange(futureEvent, pastEvent, cancelledEvent, fullEvent, eventWithDeadline, conflictingEvent, nonConflictingEvent);

        // Add registration to make event full
        _context.Registrations.Add(new Registration
        {
            Id = 1,
            EventId = 4,
            UserId = 2,
            Status = RegistrationStatus.Confirmed,
            RegisteredAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task RegisterForEventAsync_ValidRegistration_ReturnsRegistrationResponse()
    {
        // Act
        var result = await _service.RegisterForEventAsync(1, 1);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(1);
        result.UserId.Should().Be(1);
        result.Status.Should().Be(RegistrationStatus.Confirmed);
        result.Event.Should().NotBeNull();
        result.Event.Title.Should().Be("Future Event");
    }

    [Fact]
    public async Task RegisterForEventAsync_NonExistentEvent_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await _service.Invoking(s => s.RegisterForEventAsync(999, 1))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Event with ID 999 not found");
    }

    [Fact]
    public async Task RegisterForEventAsync_CancelledEvent_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await _service.Invoking(s => s.RegisterForEventAsync(3, 1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot register for a cancelled event");
    }

    [Fact]
    public async Task RegisterForEventAsync_PastEvent_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await _service.Invoking(s => s.RegisterForEventAsync(2, 1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot register for past events");
    }

    [Fact]
    public async Task RegisterForEventAsync_FullEvent_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await _service.Invoking(s => s.RegisterForEventAsync(4, 1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Event is already full");
    }

    [Fact]
    public async Task RegisterForEventAsync_PastDeadline_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await _service.Invoking(s => s.RegisterForEventAsync(5, 1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Registration deadline has passed");
    }

    [Fact]
    public async Task RegisterForEventAsync_DuplicateRegistration_ThrowsInvalidOperationException()
    {
        // Arrange - Register first time
        await _service.RegisterForEventAsync(1, 1);

        // Act & Assert - Try to register again
        await _service.Invoking(s => s.RegisterForEventAsync(1, 1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User is already registered for this event");
    }

    [Fact]
    public async Task RegisterForEventAsync_TimeConflict_ThrowsInvalidOperationException()
    {
        // Arrange - Register for first event (7 days from now, 2 hours duration)
        await _service.RegisterForEventAsync(1, 1);

        // Act & Assert - Try to register for overlapping event (7 days + 1 hour from now, 2 hours duration)
        await _service.Invoking(s => s.RegisterForEventAsync(6, 1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You have a time conflict with another registered event");
    }

    [Fact]
    public async Task RegisterForEventAsync_ReactivateCancelledRegistration_UpdatesStatus()
    {
        // Arrange - Register and then cancel
        await _service.RegisterForEventAsync(1, 1);
        await _service.CancelRegistrationAsync(1, 1);

        // Act - Register again
        var result = await _service.RegisterForEventAsync(1, 1);

        // Assert
        result.Status.Should().Be(RegistrationStatus.Confirmed);
        var registrationCount = await _context.Registrations
            .CountAsync(r => r.EventId == 1 && r.UserId == 1);
        registrationCount.Should().Be(1); // Should reuse existing registration, not create duplicate
    }

    [Fact]
    public async Task CancelRegistrationAsync_ValidCancellation_SetsStatusToCancelled()
    {
        // Arrange
        await _service.RegisterForEventAsync(1, 1);

        // Act
        await _service.CancelRegistrationAsync(1, 1);

        // Assert
        var registration = await _context.Registrations
            .FirstOrDefaultAsync(r => r.EventId == 1 && r.UserId == 1);
        registration.Should().NotBeNull();
        registration!.Status.Should().Be(RegistrationStatus.Cancelled);
    }

    [Fact]
    public async Task CancelRegistrationAsync_NonExistentRegistration_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await _service.Invoking(s => s.CancelRegistrationAsync(1, 1))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Registration not found");
    }

    [Fact]
    public async Task CancelRegistrationAsync_AlreadyCancelled_ThrowsInvalidOperationException()
    {
        // Arrange
        await _service.RegisterForEventAsync(1, 1);
        await _service.CancelRegistrationAsync(1, 1);

        // Act & Assert
        await _service.Invoking(s => s.CancelRegistrationAsync(1, 1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Registration is already cancelled");
    }

    [Fact]
    public async Task GetUserRegistrationsAsync_ReturnsAllUserRegistrations()
    {
        // Arrange
        await _service.RegisterForEventAsync(1, 1);
        await _service.RegisterForEventAsync(7, 2); // Different event, different user, non-conflicting time

        // Act
        var result = await _service.GetUserRegistrationsAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(1);
        result.First().EventId.Should().Be(1);
    }

    [Fact]
    public async Task GetUserRegistrationsAsync_IncludesCancelledRegistrations()
    {
        // Arrange
        await _service.RegisterForEventAsync(1, 1);
        await _service.CancelRegistrationAsync(1, 1);

        // Act
        var result = await _service.GetUserRegistrationsAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(RegistrationStatus.Cancelled);
    }

    [Fact]
    public async Task GetEventRegistrationsAsync_ReturnsAllEventRegistrations()
    {
        // Arrange
        await _service.RegisterForEventAsync(7, 1); // Use non-conflicting event
        await _service.RegisterForEventAsync(7, 2);

        // Act
        var result = await _service.GetEventRegistrationsAsync(7);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.UserId == 1);
        result.Should().Contain(r => r.UserId == 2);
    }

    [Fact]
    public async Task GetEventRegistrationsAsync_IncludesCancelledRegistrations()
    {
        // Arrange
        await _service.RegisterForEventAsync(7, 1); // Use non-conflicting event
        await _service.CancelRegistrationAsync(7, 1);
        await _service.RegisterForEventAsync(7, 2);

        // Act
        var result = await _service.GetEventRegistrationsAsync(7);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.UserId == 1 && r.Status == RegistrationStatus.Cancelled);
        result.Should().Contain(r => r.UserId == 2 && r.Status == RegistrationStatus.Confirmed);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
