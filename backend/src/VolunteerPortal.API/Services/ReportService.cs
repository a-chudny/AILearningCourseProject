using Microsoft.EntityFrameworkCore;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.DTOs.Reports;
using VolunteerPortal.API.Models.Enums;
using VolunteerPortal.API.Services.Interfaces;

namespace VolunteerPortal.API.Services;

/// <summary>
/// Service for generating export reports
/// </summary>
public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all active users for export with their skills
    /// </summary>
    public async Task<IEnumerable<UserExportDto>> GetUsersForExportAsync()
    {
        var users = await _context.Users
            .Where(u => !u.IsDeleted)
            .Include(u => u.UserSkills)
                .ThenInclude(us => us.Skill)
            .OrderBy(u => u.Id)
            .ToListAsync();

        return users.Select(u => new UserExportDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role.ToString(),
            Skills = string.Join(", ", u.UserSkills.Select(us => us.Skill.Name)),
            CreatedAt = u.CreatedAt
        });
    }

    /// <summary>
    /// Get all active events for export with registration counts
    /// </summary>
    public async Task<IEnumerable<EventExportDto>> GetEventsForExportAsync()
    {
        var events = await _context.Events
            .Where(e => !e.IsDeleted)
            .Include(e => e.Organizer)
            .Include(e => e.Registrations)
            .OrderBy(e => e.Id)
            .ToListAsync();

        return events.Select(e => new EventExportDto
        {
            Id = e.Id,
            Title = e.Title,
            StartTime = e.StartTime,
            Location = e.Location,
            Capacity = e.Capacity,
            RegistrationCount = e.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed),
            Status = e.Status.ToString(),
            OrganizerName = e.Organizer.Name,
            OrganizerEmail = e.Organizer.Email,
            CreatedAt = e.CreatedAt
        });
    }

    /// <summary>
    /// Get confirmed registrations for export with optional date filter
    /// </summary>
    public async Task<IEnumerable<RegistrationExportDto>> GetRegistrationsForExportAsync(
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        var query = _context.Registrations
            .Where(r => r.Status == RegistrationStatus.Confirmed)
            .Where(r => !r.Event.IsDeleted)
            .Where(r => !r.User.IsDeleted)
            .Include(r => r.Event)
            .Include(r => r.User)
            .AsQueryable();

        // Apply date filters on RegisteredAt
        if (startDate.HasValue)
        {
            query = query.Where(r => r.RegisteredAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            // Include the entire end date
            var endOfDay = endDate.Value.Date.AddDays(1);
            query = query.Where(r => r.RegisteredAt < endOfDay);
        }

        var registrations = await query
            .OrderBy(r => r.RegisteredAt)
            .ToListAsync();

        return registrations.Select(r => new RegistrationExportDto
        {
            Id = r.Id,
            EventTitle = r.Event.Title,
            EventDate = r.Event.StartTime,
            VolunteerName = r.User.Name,
            VolunteerEmail = r.User.Email,
            Status = r.Status.ToString(),
            RegisteredAt = r.RegisteredAt
        });
    }

    /// <summary>
    /// Get skills summary with volunteer and event counts
    /// </summary>
    public async Task<IEnumerable<SkillsSummaryDto>> GetSkillsSummaryForExportAsync()
    {
        var skills = await _context.Skills
            .Include(s => s.UserSkills)
                .ThenInclude(us => us.User)
            .Include(s => s.EventSkills)
                .ThenInclude(es => es.Event)
            .OrderBy(s => s.Id)
            .ToListAsync();

        return skills.Select(s => new SkillsSummaryDto
        {
            Id = s.Id,
            SkillName = s.Name,
            Description = s.Description,
            VolunteerCount = s.UserSkills.Count(us => !us.User.IsDeleted),
            EventCount = s.EventSkills.Count(es => !es.Event.IsDeleted)
        });
    }
}
