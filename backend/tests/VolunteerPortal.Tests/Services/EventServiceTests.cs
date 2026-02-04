using Microsoft.EntityFrameworkCore;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.DTOs.Events;
using VolunteerPortal.API.Models.Entities;
using VolunteerPortal.API.Models.Enums;
using VolunteerPortal.API.Services;

namespace VolunteerPortal.Tests.Services;

public class EventServiceTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        return context;
    }

    private async Task<User> SeedOrganizerAsync(ApplicationDbContext context, UserRole role = UserRole.Organizer)
    {
        var user = new User
        {
            Email = "organizer@test.com",
            Name = "Test Organizer",
            PasswordHash = "hashedpassword",
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private async Task<Skill> SeedSkillAsync(ApplicationDbContext context)
    {
        var skill = new Skill
        {
            Name = "Test Skill",
            Description = "Test Category" // Using Description instead of Category
        };

        context.Skills.Add(skill);
        await context.SaveChangesAsync();
        return skill;
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesEvent()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);

        var request = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };

        // Act
        var result = await service.CreateAsync(request, organizer.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Event", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal("Test Location", result.Location);
        Assert.Equal(120, result.DurationMinutes);
        Assert.Equal(50, result.Capacity);
        Assert.Equal(EventStatus.Active, result.Status);
        Assert.Equal(organizer.Id, result.OrganizerId);
        Assert.Equal(organizer.Name, result.OrganizerName);
        Assert.Equal(0, result.RegistrationCount);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidOrganizer_ThrowsArgumentException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);

        var request = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(request, 999));
    }

    [Fact]
    public async Task CreateAsync_WithPastStartTime_ThrowsArgumentException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);

        var request = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(-1),
            DurationMinutes = 120,
            Capacity = 50
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(request, organizer.Id));
    }

    [Fact]
    public async Task CreateAsync_WithInvalidRegistrationDeadline_ThrowsArgumentException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);

        var startTime = DateTime.UtcNow.AddDays(7);
        var request = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = startTime,
            DurationMinutes = 120,
            Capacity = 50,
            RegistrationDeadline = startTime.AddDays(1) // After start time
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(request, organizer.Id));
    }

    [Fact]
    public async Task CreateAsync_WithRequiredSkills_AssociatesSkills()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);
        var skill = await SeedSkillAsync(context);

        var request = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50,
            RequiredSkillIds = new List<int> { skill.Id }
        };

        // Act
        var result = await service.CreateAsync(request, organizer.Id);

        // Assert
        Assert.Single(result.RequiredSkills);
        Assert.Equal(skill.Name, result.RequiredSkills[0].Name);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidRequest_UpdatesEvent()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);

        var createRequest = new CreateEventRequest
        {
            Title = "Original Title",
            Description = "Original Description",
            Location = "Original Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };

        var created = await service.CreateAsync(createRequest, organizer.Id);

        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Location = "Updated Location",
            StartTime = DateTime.UtcNow.AddDays(10),
            DurationMinutes = 180,
            Capacity = 75,
            Status = EventStatus.Active
        };

        // Act
        var result = await service.UpdateAsync(created.Id, updateRequest, organizer.Id);

        // Assert
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal("Updated Location", result.Location);
        Assert.Equal(180, result.DurationMinutes);
        Assert.Equal(75, result.Capacity);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_WithNonexistentEvent_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);

        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Location = "Updated Location",
            StartTime = DateTime.UtcNow.AddDays(10),
            DurationMinutes = 180,
            Capacity = 75,
            Status = EventStatus.Active
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateAsync(999, updateRequest, organizer.Id));
    }

    [Fact]
    public async Task UpdateAsync_WithNonOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);
        var otherUser = new User
        {
            Email = "other@test.com",
            Name = "Other User",
            PasswordHash = "hashedpassword",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(otherUser);
        await context.SaveChangesAsync();

        var createRequest = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };

        var created = await service.CreateAsync(createRequest, organizer.Id);

        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Location = "Updated Location",
            StartTime = DateTime.UtcNow.AddDays(10),
            DurationMinutes = 180,
            Capacity = 75,
            Status = EventStatus.Active
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.UpdateAsync(created.Id, updateRequest, otherUser.Id));
    }

    [Fact]
    public async Task UpdateAsync_ByAdmin_Succeeds()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);
        var admin = new User
        {
            Email = "admin@test.com",
            Name = "Admin User",
            PasswordHash = "hashedpassword",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var createRequest = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };

        var created = await service.CreateAsync(createRequest, organizer.Id);

        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated by Admin",
            Description = "Updated Description",
            Location = "Updated Location",
            StartTime = DateTime.UtcNow.AddDays(10),
            DurationMinutes = 180,
            Capacity = 75,
            Status = EventStatus.Active
        };

        // Act
        var result = await service.UpdateAsync(created.Id, updateRequest, admin.Id);

        // Assert
        Assert.Equal("Updated by Admin", result.Title);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesRequiredSkills()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);
        var skill1 = await SeedSkillAsync(context);
        var skill2 = new Skill
        {
            Name = "Second Skill",
            Description = "Second Category" // Using Description instead of Category
        };
        context.Skills.Add(skill2);
        await context.SaveChangesAsync();

        var createRequest = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50,
            RequiredSkillIds = new List<int> { skill1.Id }
        };

        var created = await service.CreateAsync(createRequest, organizer.Id);

        var updateRequest = new UpdateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50,
            Status = EventStatus.Active,
            RequiredSkillIds = new List<int> { skill2.Id }
        };

        // Act
        var result = await service.UpdateAsync(created.Id, updateRequest, organizer.Id);

        // Assert
        Assert.Single(result.RequiredSkills);
        Assert.Equal(skill2.Name, result.RequiredSkills[0].Name);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidOwner_SoftDeletesEvent()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);

        var createRequest = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };

        var created = await service.CreateAsync(createRequest, organizer.Id);

        // Act
        await service.DeleteAsync(created.Id, organizer.Id);

        // Assert
        var deleted = await service.GetByIdAsync(created.Id);
        Assert.Null(deleted); // Soft-deleted events are not returned

        var eventInDb = await context.Events.FindAsync(created.Id);
        Assert.True(eventInDb!.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_WithNonexistentEvent_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteAsync(999, organizer.Id));
    }

    [Fact]
    public async Task DeleteAsync_WithNonOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);
        var otherUser = new User
        {
            Email = "other@test.com",
            Name = "Other User",
            PasswordHash = "hashedpassword",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(otherUser);
        await context.SaveChangesAsync();

        var createRequest = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };

        var created = await service.CreateAsync(createRequest, organizer.Id);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.DeleteAsync(created.Id, otherUser.Id));
    }

    [Fact]
    public async Task DeleteAsync_ByAdmin_Succeeds()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);
        var admin = new User
        {
            Email = "admin@test.com",
            Name = "Admin User",
            PasswordHash = "hashedpassword",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var createRequest = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };

        var created = await service.CreateAsync(createRequest, organizer.Id);

        // Act
        await service.DeleteAsync(created.Id, admin.Id);

        // Assert
        var deleted = await service.GetByIdAsync(created.Id);
        Assert.Null(deleted);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsEvent()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);

        var createRequest = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };

        var created = await service.CreateAsync(createRequest, organizer.Id);

        // Act
        var result = await service.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Test Event", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonexistentId_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithSoftDeletedEvent_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);

        var createRequest = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };

        var created = await service.CreateAsync(createRequest, organizer.Id);
        await service.DeleteAsync(created.Id, organizer.Id);

        // Act
        var result = await service.GetByIdAsync(created.Id);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsUpcomingEvents()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);

        // Create upcoming event
        var upcomingRequest = new CreateEventRequest
        {
            Title = "Upcoming Event",
            Description = "Description",
            Location = "Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };
        await service.CreateAsync(upcomingRequest, organizer.Id);

        // Create past event directly in database
        var pastEvent = new Event
        {
            Title = "Past Event",
            Description = "Description",
            Location = "Location",
            StartTime = DateTime.UtcNow.AddDays(-7),
            DurationMinutes = 120,
            Capacity = 50,
            Status = EventStatus.Active,
            OrganizerId = organizer.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        context.Events.Add(pastEvent);
        await context.SaveChangesAsync();

        var queryParams = new EventQueryParams
        {
            Page = 1,
            PageSize = 20
        };

        // Act
        var result = await service.GetAllAsync(queryParams);

        // Assert
        Assert.Single(result.Events);
        Assert.Equal("Upcoming Event", result.Events[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_WithIncludePastEvents_ReturnsAllEvents()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);

        // Create upcoming event
        var upcomingRequest = new CreateEventRequest
        {
            Title = "Upcoming Event",
            Description = "Description",
            Location = "Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };
        await service.CreateAsync(upcomingRequest, organizer.Id);

        // Create past event
        var pastEvent = new Event
        {
            Title = "Past Event",
            Description = "Description",
            Location = "Location",
            StartTime = DateTime.UtcNow.AddDays(-7),
            DurationMinutes = 120,
            Capacity = 50,
            Status = EventStatus.Active,
            OrganizerId = organizer.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        context.Events.Add(pastEvent);
        await context.SaveChangesAsync();

        var queryParams = new EventQueryParams
        {
            Page = 1,
            PageSize = 20,
            IncludePastEvents = true
        };

        // Act
        var result = await service.GetAllAsync(queryParams);

        // Assert
        Assert.Equal(2, result.Events.Count);
    }

    [Fact]
    public async Task GetAllAsync_WithSearchTerm_FiltersEvents()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);

        var request1 = new CreateEventRequest
        {
            Title = "Beach Cleanup",
            Description = "Clean the beach",
            Location = "Beach",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };
        await service.CreateAsync(request1, organizer.Id);

        var request2 = new CreateEventRequest
        {
            Title = "Park Maintenance",
            Description = "Maintain the park",
            Location = "Park",
            StartTime = DateTime.UtcNow.AddDays(8),
            DurationMinutes = 120,
            Capacity = 50
        };
        await service.CreateAsync(request2, organizer.Id);

        var queryParams = new EventQueryParams
        {
            Page = 1,
            PageSize = 20,
            SearchTerm = "beach"
        };

        // Act
        var result = await service.GetAllAsync(queryParams);

        // Assert
        Assert.Single(result.Events);
        Assert.Equal("Beach Cleanup", result.Events[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);

        // Create 25 events
        for (int i = 1; i <= 25; i++)
        {
            var request = new CreateEventRequest
            {
                Title = $"Event {i}",
                Description = "Description",
                Location = "Location",
                StartTime = DateTime.UtcNow.AddDays(i),
                DurationMinutes = 120,
                Capacity = 50
            };
            await service.CreateAsync(request, organizer.Id);
        }

        var queryParams = new EventQueryParams
        {
            Page = 2,
            PageSize = 10
        };

        // Act
        var result = await service.GetAllAsync(queryParams);

        // Assert
        Assert.Equal(10, result.Events.Count);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_FiltersCorrectly()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);

        var activeRequest = new CreateEventRequest
        {
            Title = "Active Event",
            Description = "Description",
            Location = "Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };
        var activeEvent = await service.CreateAsync(activeRequest, organizer.Id);

        // Create cancelled event
        var cancelledEvent = new Event
        {
            Title = "Cancelled Event",
            Description = "Description",
            Location = "Location",
            StartTime = DateTime.UtcNow.AddDays(8),
            DurationMinutes = 120,
            Capacity = 50,
            Status = EventStatus.Cancelled,
            OrganizerId = organizer.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.Events.Add(cancelledEvent);
        await context.SaveChangesAsync();

        var queryParams = new EventQueryParams
        {
            Page = 1,
            PageSize = 20,
            Status = EventStatus.Active
        };

        // Act
        var result = await service.GetAllAsync(queryParams);

        // Assert
        Assert.Single(result.Events);
        Assert.Equal("Active Event", result.Events[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_IncludesRegistrationCount()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new EventService(context);
        var organizer = await SeedOrganizerAsync(context);
        var volunteer = new User
        {
            Email = "volunteer@test.com",
            Name = "Volunteer",
            PasswordHash = "hashedpassword",
            Role = UserRole.Volunteer,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(volunteer);
        await context.SaveChangesAsync();

        var createRequest = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Description",
            Location = "Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };
        var created = await service.CreateAsync(createRequest, organizer.Id);

        // Add registration
        var registration = new Registration
        {
            EventId = created.Id,
            UserId = volunteer.Id,
            Status = RegistrationStatus.Confirmed,
            RegisteredAt = DateTime.UtcNow
        };
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        var queryParams = new EventQueryParams
        {
            Page = 1,
            PageSize = 20
        };

        // Act
        var result = await service.GetAllAsync(queryParams);

        // Assert
        Assert.Single(result.Events);
        Assert.Equal(1, result.Events[0].RegistrationCount);
    }

    #endregion
}
