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
    /// Creates: 1 Admin, 1 Organizer, 1 Volunteer
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
            new()
            {
                Name = "System Administrator",
                Email = "admin@portal.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin"),
                Role = UserRole.Admin,
                PhoneNumber = "+1-555-0001",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new()
            {
                Name = "Community Organizer",
                Email = "organizer@portal.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Organizer"),
                Role = UserRole.Organizer,
                PhoneNumber = "+1-555-0002",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new()
            {
                Name = "John Volunteer",
                Email = "volunteer@portal.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Volunteer"),
                Role = UserRole.Volunteer,
                PhoneNumber = "+1-555-0003",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
    }
}
