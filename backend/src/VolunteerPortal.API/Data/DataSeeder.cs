using Microsoft.EntityFrameworkCore;
using VolunteerPortal.API.Models.Entities;
using VolunteerPortal.API.Models.Enums;
using BCrypt.Net;

namespace VolunteerPortal.API.Data;

/// <summary>
/// Seeds initial data into the database for development and testing.
/// </summary>
public class DataSeeder
{
    private readonly ApplicationDbContext _context;

    public DataSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Seeds all required data. This method is idempotent and can be run multiple times safely.
    /// </summary>
    public async Task SeedAsync()
    {
        await SeedSkillsAsync();
        await SeedUsersAsync();
        await SeedUserSkillsAsync();
        await SeedEventsAsync();
        await SeedEventSkillsAsync();
        await SeedRegistrationsAsync();
    }

    /// <summary>
    /// Seeds the predefined skills list with categories.
    /// </summary>
    private async Task SeedSkillsAsync()
    {
        // Check if skills already exist
        if (await _context.Skills.AnyAsync())
        {
            return; // Skills already seeded
        }

        var skills = new List<Skill>
        {
            new() { Name = "First Aid / CPR", Description = "Medical", CreatedAt = DateTime.UtcNow },
            new() { Name = "Teaching / Tutoring", Description = "Education", CreatedAt = DateTime.UtcNow },
            new() { Name = "Driving", Description = "Transportation", CreatedAt = DateTime.UtcNow },
            new() { Name = "Cooking / Food Preparation", Description = "Food Service", CreatedAt = DateTime.UtcNow },
            new() { Name = "Event Setup / Cleanup", Description = "General Labor", CreatedAt = DateTime.UtcNow },
            new() { Name = "Photography / Videography", Description = "Media", CreatedAt = DateTime.UtcNow },
            new() { Name = "Social Media / Marketing", Description = "Communications", CreatedAt = DateTime.UtcNow },
            new() { Name = "Translation / Interpretation", Description = "Languages", CreatedAt = DateTime.UtcNow },
            new() { Name = "IT / Technical Support", Description = "Technology", CreatedAt = DateTime.UtcNow },
            new() { Name = "Childcare", Description = "Care", CreatedAt = DateTime.UtcNow },
            new() { Name = "Senior Care", Description = "Care", CreatedAt = DateTime.UtcNow },
            new() { Name = "Gardening / Landscaping", Description = "Outdoor", CreatedAt = DateTime.UtcNow },
            new() { Name = "Construction / Repair", Description = "Skilled Trade", CreatedAt = DateTime.UtcNow },
            new() { Name = "Counseling / Mentoring", Description = "Support", CreatedAt = DateTime.UtcNow },
            new() { Name = "Administrative / Office Work", Description = "Office", CreatedAt = DateTime.UtcNow }
        };

        await _context.Skills.AddRangeAsync(skills);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds initial users for testing and development.
    /// Creates: 1 Admin, 2 Organizers, 4 Volunteers
    /// </summary>
    private async Task SeedUsersAsync()
    {
        // Check if users already exist
        if (await _context.Users.AnyAsync())
        {
            return; // Users already seeded
        }

        var users = new List<User>
        {
            // Admin
            new()
            {
                Name = "System Administrator",
                Email = "admin@portal.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                PhoneNumber = "+995-555-100001",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            // Organizers
            new()
            {
                Name = "Nino Beridze",
                Email = "nino.organizer@portal.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Organizer123!"),
                Role = UserRole.Organizer,
                PhoneNumber = "+995-555-200001",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new()
            {
                Name = "Giorgi Tsiklauri",
                Email = "giorgi.organizer@portal.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Organizer123!"),
                Role = UserRole.Organizer,
                PhoneNumber = "+995-555-200002",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            // Volunteers (with skills - will be added in SeedUserSkillsAsync)
            new()
            {
                Name = "Mariam Kapanadze",
                Email = "mariam.volunteer@portal.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Volunteer123!"),
                Role = UserRole.Volunteer,
                PhoneNumber = "+995-555-300001",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new()
            {
                Name = "Davit Lomidze",
                Email = "davit.volunteer@portal.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Volunteer123!"),
                Role = UserRole.Volunteer,
                PhoneNumber = "+995-555-300002",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            // Volunteers (without skills)
            new()
            {
                Name = "Luka Janelidze",
                Email = "luka.volunteer@portal.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Volunteer123!"),
                Role = UserRole.Volunteer,
                PhoneNumber = "+995-555-300003",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new()
            {
                Name = "Ana Kvaratskhelia",
                Email = "ana.volunteer@portal.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Volunteer123!"),
                Role = UserRole.Volunteer,
                PhoneNumber = "+995-555-300004",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds user skills for volunteers with skills.
    /// </summary>
    private async Task SeedUserSkillsAsync()
    {
        if (await _context.UserSkills.AnyAsync())
        {
            return; // Already seeded
        }

        // Get volunteers with skills
        var mariam = await _context.Users.FirstOrDefaultAsync(u => u.Email == "mariam.volunteer@portal.com");
        var davit = await _context.Users.FirstOrDefaultAsync(u => u.Email == "davit.volunteer@portal.com");

        if (mariam == null || davit == null) return;

        // Get skills
        var firstAid = await _context.Skills.FirstOrDefaultAsync(s => s.Name == "First Aid / CPR");
        var cooking = await _context.Skills.FirstOrDefaultAsync(s => s.Name == "Cooking / Food Preparation");
        var eventSetup = await _context.Skills.FirstOrDefaultAsync(s => s.Name == "Event Setup / Cleanup");
        var driving = await _context.Skills.FirstOrDefaultAsync(s => s.Name == "Driving");
        var photography = await _context.Skills.FirstOrDefaultAsync(s => s.Name == "Photography / Videography");

        if (firstAid == null || cooking == null || eventSetup == null || driving == null || photography == null) return;

        var userSkills = new List<UserSkill>
        {
            // Mariam has First Aid, Cooking, Event Setup
            new() { UserId = mariam.Id, SkillId = firstAid.Id, AddedAt = DateTime.UtcNow },
            new() { UserId = mariam.Id, SkillId = cooking.Id, AddedAt = DateTime.UtcNow },
            new() { UserId = mariam.Id, SkillId = eventSetup.Id, AddedAt = DateTime.UtcNow },
            // Davit has Driving, Photography, Event Setup
            new() { UserId = davit.Id, SkillId = driving.Id, AddedAt = DateTime.UtcNow },
            new() { UserId = davit.Id, SkillId = photography.Id, AddedAt = DateTime.UtcNow },
            new() { UserId = davit.Id, SkillId = eventSetup.Id, AddedAt = DateTime.UtcNow }
        };

        await _context.UserSkills.AddRangeAsync(userSkills);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds events for testing. Creates 4 events:
    /// - 1 past event, 1 in 3 days, 1 in 5 days, 1 in 7 days
    /// - 2 events require skills, 2 don't
    /// - 2 events have images
    /// All locations are in Tbilisi, Georgia
    /// </summary>
    private async Task SeedEventsAsync()
    {
        if (await _context.Events.AnyAsync())
        {
            return; // Already seeded
        }

        // Get organizers
        var nino = await _context.Users.FirstOrDefaultAsync(u => u.Email == "nino.organizer@portal.com");
        var giorgi = await _context.Users.FirstOrDefaultAsync(u => u.Email == "giorgi.organizer@portal.com");

        if (nino == null || giorgi == null) return;

        var today = DateTime.UtcNow.Date;

        var events = new List<Event>
        {
            // Event 1: Past event (5 days ago) - Community park cleanup in Vake Park
            new()
            {
                Title = "Vake Park Community Cleanup",
                Description = "Join us for a community cleanup day at Vake Park! We'll be collecting litter, " +
                    "planting flowers near the main fountain, and beautifying the walking paths. " +
                    "Gloves and trash bags will be provided. Please wear comfortable clothes and bring your own water bottle. " +
                    "This is a great opportunity to meet fellow volunteers and make a visible impact in one of Tbilisi's most beloved parks. " +
                    "Families with children are welcome!",
                Location = "Vake Park, I. Chavchavadze Ave, Tbilisi 0179, Georgia",
                StartTime = today.AddDays(-5).AddHours(10), // 10:00 AM, 5 days ago
                DurationMinutes = 180, // 3 hours
                Capacity = 30,
                ImageUrl = "/uploads/events/community-cleanup.jpg",
                RegistrationDeadline = today.AddDays(-6), // Deadline was day before
                Status = EventStatus.Active,
                OrganizerId = nino.Id,
                CreatedAt = today.AddDays(-15),
                IsDeleted = false
            },
            // Event 2: In 3 days - Food distribution (requires Cooking skill)
            new()
            {
                Title = "Dezerter Bazaar Food Distribution",
                Description = "Help us distribute warm meals to those in need at Dezerter Bazaar area. " +
                    "Volunteers will assist with food preparation, packaging, and distribution. " +
                    "We'll be serving traditional Georgian soup (kharcho) and fresh bread. " +
                    "Experience in food handling preferred but not required - we'll provide training on-site. " +
                    "This event is organized in partnership with Tbilisi Municipality's social services program.",
                Location = "Dezerter Bazaar, Station Square, Tbilisi 0112, Georgia",
                StartTime = today.AddDays(3).AddHours(11), // 11:00 AM, 3 days from now
                DurationMinutes = 240, // 4 hours
                Capacity = 15,
                ImageUrl = "/uploads/events/food-distribution.jpg",
                RegistrationDeadline = today.AddDays(2), // Day before event
                Status = EventStatus.Active,
                OrganizerId = nino.Id,
                CreatedAt = today.AddDays(-7),
                IsDeleted = false
            },
            // Event 3: In 5 days - First Aid training (requires First Aid skill)
            new()
            {
                Title = "Youth First Aid Training Workshop",
                Description = "Volunteer as a trainer or assistant at our youth first aid workshop! " +
                    "We're teaching basic first aid and CPR skills to high school students from public schools. " +
                    "Experienced first aid certified volunteers needed to demonstrate techniques and supervise practice sessions. " +
                    "The workshop includes hands-on training with mannequins and first aid kits. " +
                    "All materials provided by Georgian Red Cross Society. Light refreshments will be served.",
                Location = "Tbilisi Youth Palace, 6 Gudiashvili Street, Tbilisi 0107, Georgia",
                StartTime = today.AddDays(5).AddHours(14), // 2:00 PM, 5 days from now
                DurationMinutes = 180, // 3 hours
                Capacity = 8,
                ImageUrl = null, // No image
                RegistrationDeadline = today.AddDays(4),
                Status = EventStatus.Active,
                OrganizerId = giorgi.Id,
                CreatedAt = today.AddDays(-5),
                IsDeleted = false
            },
            // Event 4: In 7 days - Elderly home visit (no skills required)
            new()
            {
                Title = "Elderly Care Home Visit & Entertainment",
                Description = "Spend a meaningful afternoon with residents of Tbilisi Senior Care Center! " +
                    "Activities include conversation, board games, reading, and light entertainment. " +
                    "No special skills required - just a warm heart and willingness to listen. " +
                    "This visit coincides with International Day of Older Persons celebrations. " +
                    "We'll bring small gifts and organize a mini-concert with local musicians. " +
                    "Transportation from Rustaveli Metro station will be arranged for volunteers.",
                Location = "Tbilisi Senior Care Center, 45 Tsereteli Avenue, Tbilisi 0154, Georgia",
                StartTime = today.AddDays(7).AddHours(15), // 3:00 PM, 7 days from now
                DurationMinutes = 150, // 2.5 hours
                Capacity = 20,
                ImageUrl = null, // No image
                RegistrationDeadline = today.AddDays(6),
                Status = EventStatus.Active,
                OrganizerId = giorgi.Id,
                CreatedAt = today.AddDays(-3),
                IsDeleted = false
            }
        };

        await _context.Events.AddRangeAsync(events);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds required skills for events that need them.
    /// </summary>
    private async Task SeedEventSkillsAsync()
    {
        if (await _context.EventSkills.AnyAsync())
        {
            return; // Already seeded
        }

        // Get events that require skills
        var foodEvent = await _context.Events.FirstOrDefaultAsync(e => e.Title == "Dezerter Bazaar Food Distribution");
        var firstAidEvent = await _context.Events.FirstOrDefaultAsync(e => e.Title == "Youth First Aid Training Workshop");

        if (foodEvent == null || firstAidEvent == null) return;

        // Get required skills
        var cooking = await _context.Skills.FirstOrDefaultAsync(s => s.Name == "Cooking / Food Preparation");
        var firstAid = await _context.Skills.FirstOrDefaultAsync(s => s.Name == "First Aid / CPR");

        if (cooking == null || firstAid == null) return;

        var eventSkills = new List<EventSkill>
        {
            new() { EventId = foodEvent.Id, SkillId = cooking.Id },
            new() { EventId = firstAidEvent.Id, SkillId = firstAid.Id }
        };

        await _context.EventSkills.AddRangeAsync(eventSkills);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds registrations - some volunteers are registered for events.
    /// </summary>
    private async Task SeedRegistrationsAsync()
    {
        if (await _context.Registrations.AnyAsync())
        {
            return; // Already seeded
        }

        // Get volunteers
        var mariam = await _context.Users.FirstOrDefaultAsync(u => u.Email == "mariam.volunteer@portal.com");
        var davit = await _context.Users.FirstOrDefaultAsync(u => u.Email == "davit.volunteer@portal.com");

        if (mariam == null || davit == null) return;

        // Get events
        var foodEvent = await _context.Events.FirstOrDefaultAsync(e => e.Title == "Dezerter Bazaar Food Distribution");
        var elderlyEvent = await _context.Events.FirstOrDefaultAsync(e => e.Title == "Elderly Care Home Visit & Entertainment");

        if (foodEvent == null || elderlyEvent == null) return;

        var registrations = new List<Registration>
        {
            // Mariam registered for food distribution (she has cooking skill)
            new()
            {
                EventId = foodEvent.Id,
                UserId = mariam.Id,
                Status = RegistrationStatus.Confirmed,
                RegisteredAt = DateTime.UtcNow.AddDays(-2),
                Notes = "I have experience with food preparation and can help with cooking."
            },
            // Davit registered for elderly home visit
            new()
            {
                EventId = elderlyEvent.Id,
                UserId = davit.Id,
                Status = RegistrationStatus.Confirmed,
                RegisteredAt = DateTime.UtcNow.AddDays(-1),
                Notes = "I can bring my camera to take photos of the event."
            }
        };

        await _context.Registrations.AddRangeAsync(registrations);
        await _context.SaveChangesAsync();
    }
}
