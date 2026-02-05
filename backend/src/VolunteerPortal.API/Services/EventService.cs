using Microsoft.EntityFrameworkCore;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.DTOs.Events;
using VolunteerPortal.API.Models.Entities;
using VolunteerPortal.API.Models.Enums;
using VolunteerPortal.API.Services.Interfaces;

namespace VolunteerPortal.API.Services;

/// <summary>
/// Service for managing event operations.
/// </summary>
public class EventService : IEventService
{
    private readonly ApplicationDbContext _context;

    public EventService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<EventResponse> CreateAsync(CreateEventRequest request, int organizerId)
    {
        // Validate organizer exists
        var organizer = await _context.Users
            .Where(u => u.Id == organizerId && !u.IsDeleted)
            .FirstOrDefaultAsync();

        if (organizer == null)
        {
            throw new ArgumentException("Organizer not found.", nameof(organizerId));
        }

        // Validate start time is in the future
        if (request.StartTime <= DateTime.UtcNow)
        {
            throw new ArgumentException("Event start time must be in the future.", nameof(request.StartTime));
        }

        // Validate registration deadline if provided
        if (request.RegistrationDeadline.HasValue && request.RegistrationDeadline.Value >= request.StartTime)
        {
            throw new ArgumentException("Registration deadline must be before the event start time.", nameof(request.RegistrationDeadline));
        }

        // Create event entity
        var eventEntity = new Event
        {
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            StartTime = request.StartTime,
            DurationMinutes = request.DurationMinutes,
            Capacity = request.Capacity,
            ImageUrl = request.ImageUrl,
            RegistrationDeadline = request.RegistrationDeadline,
            Status = EventStatus.Active,
            OrganizerId = organizerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        // Handle required skills if provided
        if (request.RequiredSkillIds.Any())
        {
            var validSkillIds = await _context.Skills
                .Where(s => request.RequiredSkillIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();

            foreach (var skillId in validSkillIds)
            {
                _context.EventSkills.Add(new EventSkill
                {
                    EventId = eventEntity.Id,
                    SkillId = skillId
                });
            }

            await _context.SaveChangesAsync();
        }

        return await GetByIdAsync(eventEntity.Id) 
            ?? throw new InvalidOperationException("Failed to retrieve created event.");
    }

    /// <inheritdoc />
    public async Task<EventResponse> UpdateAsync(int id, UpdateEventRequest request, int userId)
    {
        // Get event with organizer info
        var eventEntity = await _context.Events
            .Include(e => e.Organizer)
            .Where(e => e.Id == id && !e.IsDeleted)
            .FirstOrDefaultAsync();

        if (eventEntity == null)
        {
            throw new KeyNotFoundException($"Event with ID {id} not found.");
        }

        // Get user to check if admin
        var user = await _context.Users
            .Where(u => u.Id == userId && !u.IsDeleted)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        // Check ownership (owner or admin can update)
        if (eventEntity.OrganizerId != userId && user.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this event.");
        }

        // Validate start time is in the future (unless admin)
        if (request.StartTime <= DateTime.UtcNow && user.Role != UserRole.Admin)
        {
            throw new ArgumentException("Event start time must be in the future.", nameof(request.StartTime));
        }

        // Validate registration deadline if provided
        if (request.RegistrationDeadline.HasValue && request.RegistrationDeadline.Value >= request.StartTime)
        {
            throw new ArgumentException("Registration deadline must be before the event start time.", nameof(request.RegistrationDeadline));
        }

        // Update event properties
        eventEntity.Title = request.Title;
        eventEntity.Description = request.Description;
        eventEntity.Location = request.Location;
        eventEntity.StartTime = request.StartTime;
        eventEntity.DurationMinutes = request.DurationMinutes;
        eventEntity.Capacity = request.Capacity;
        eventEntity.ImageUrl = request.ImageUrl;
        eventEntity.RegistrationDeadline = request.RegistrationDeadline;
        eventEntity.Status = request.Status;
        eventEntity.UpdatedAt = DateTime.UtcNow;

        // Update required skills
        var existingSkills = await _context.EventSkills
            .Where(es => es.EventId == id)
            .ToListAsync();

        _context.EventSkills.RemoveRange(existingSkills);

        if (request.RequiredSkillIds.Any())
        {
            var validSkillIds = await _context.Skills
                .Where(s => request.RequiredSkillIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();

            foreach (var skillId in validSkillIds)
            {
                _context.EventSkills.Add(new EventSkill
                {
                    EventId = id,
                    SkillId = skillId
                });
            }
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id) 
            ?? throw new InvalidOperationException("Failed to retrieve updated event.");
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, int userId)
    {
        // Get event
        var eventEntity = await _context.Events
            .Where(e => e.Id == id && !e.IsDeleted)
            .FirstOrDefaultAsync();

        if (eventEntity == null)
        {
            throw new KeyNotFoundException($"Event with ID {id} not found.");
        }

        // Get user to check if admin
        var user = await _context.Users
            .Where(u => u.Id == userId && !u.IsDeleted)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        // Check ownership (owner or admin can delete)
        if (eventEntity.OrganizerId != userId && user.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this event.");
        }

        // Soft delete
        eventEntity.IsDeleted = true;
        eventEntity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<EventResponse?> GetByIdAsync(int id)
    {
        var eventEntity = await _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Registrations)
            .Include(e => e.EventSkills)
                .ThenInclude(es => es.Skill)
            .Where(e => e.Id == id && !e.IsDeleted)
            .FirstOrDefaultAsync();

        if (eventEntity == null)
        {
            return null;
        }

        return MapToResponse(eventEntity);
    }

    /// <inheritdoc />
    public async Task<EventListResponse> GetAllAsync(EventQueryParams queryParams, int? currentUserId = null)
    {
        // Start with base query
        var query = _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Registrations)
            .Include(e => e.EventSkills)
                .ThenInclude(es => es.Skill)
            .AsQueryable();

        // Filter soft-deleted events (unless explicitly included)
        if (!queryParams.IncludeDeleted)
        {
            query = query.Where(e => !e.IsDeleted);
        }

        // Filter by upcoming/past events
        if (!queryParams.IncludePastEvents)
        {
            query = query.Where(e => e.StartTime > DateTime.UtcNow);
        }

        // Filter by status if specified
        if (queryParams.Status.HasValue)
        {
            query = query.Where(e => e.Status == queryParams.Status.Value);
        }

        // Search by title or description
        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var searchTerm = queryParams.SearchTerm.ToLower();
            query = query.Where(e => 
                e.Title.ToLower().Contains(searchTerm) || 
                e.Description.ToLower().Contains(searchTerm));
        }

        // Filter by skills - match ANY of the specified skills
        if (queryParams.SkillIds != null && queryParams.SkillIds.Any())
        {
            query = query.Where(e => e.EventSkills.Any(es => queryParams.SkillIds.Contains(es.SkillId)));
        }

        // Filter by user's skills - match ANY of user's skills
        if (queryParams.MatchMySkills && currentUserId.HasValue)
        {
            var userSkillIds = await _context.UserSkills
                .Where(us => us.UserId == currentUserId.Value)
                .Select(us => us.SkillId)
                .ToListAsync();

            if (userSkillIds.Any())
            {
                query = query.Where(e => e.EventSkills.Any(es => userSkillIds.Contains(es.SkillId)));
            }
            else
            {
                // User has no skills, return no events
                query = query.Where(e => false);
            }
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = queryParams.SortBy.ToLower() switch
        {
            "title" => queryParams.SortDirection.ToLower() == "desc" 
                ? query.OrderByDescending(e => e.Title) 
                : query.OrderBy(e => e.Title),
            "createdat" => queryParams.SortDirection.ToLower() == "desc" 
                ? query.OrderByDescending(e => e.CreatedAt) 
                : query.OrderBy(e => e.CreatedAt),
            _ => queryParams.SortDirection.ToLower() == "desc" 
                ? query.OrderByDescending(e => e.StartTime) 
                : query.OrderBy(e => e.StartTime)
        };

        // Apply pagination
        var skip = (queryParams.Page - 1) * queryParams.PageSize;
        var events = await query
            .Skip(skip)
            .Take(queryParams.PageSize)
            .ToListAsync();

        // Map to response
        var eventResponses = events.Select(MapToResponse).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)queryParams.PageSize);

        return new EventListResponse
        {
            Events = eventResponses,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = queryParams.Page > 1,
            HasNextPage = queryParams.Page < totalPages
        };
    }

    /// <summary>
    /// Maps an Event entity to EventResponse DTO.
    /// </summary>
    private EventResponse MapToResponse(Event eventEntity)
    {
        var registrationCount = eventEntity.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed);
        var availableSpots = eventEntity.Capacity - registrationCount;
        var isFull = registrationCount >= eventEntity.Capacity;

        return new EventResponse
        {
            Id = eventEntity.Id,
            Title = eventEntity.Title,
            Description = eventEntity.Description,
            Location = eventEntity.Location,
            StartTime = eventEntity.StartTime,
            DurationMinutes = eventEntity.DurationMinutes,
            Capacity = eventEntity.Capacity,
            ImageUrl = eventEntity.ImageUrl,
            RegistrationDeadline = eventEntity.RegistrationDeadline,
            Status = eventEntity.Status,
            RegistrationCount = registrationCount,
            AvailableSpots = availableSpots,
            IsFull = isFull,
            OrganizerId = eventEntity.OrganizerId,
            OrganizerName = eventEntity.Organizer.Name,
            OrganizerEmail = eventEntity.Organizer.Email,
            CreatedAt = eventEntity.CreatedAt,
            UpdatedAt = eventEntity.UpdatedAt,
            IsDeleted = eventEntity.IsDeleted,
            RequiredSkills = eventEntity.EventSkills
                .Select(es => new Models.DTOs.SkillResponse
                {
                    Id = es.Skill.Id,
                    Name = es.Skill.Name,
                    Category = es.Skill.Description ?? "General" // Use Description or default
                })
                .ToList()
        };
    }
}
