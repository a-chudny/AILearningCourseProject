using Microsoft.EntityFrameworkCore;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.Entities;
using VolunteerPortal.API.Models.Enums;
using VolunteerPortal.API.Models.DTOs.Registrations;
using VolunteerPortal.API.Services.Interfaces;

namespace VolunteerPortal.API.Services;

/// <summary>
/// Service for managing event registrations
/// </summary>
public class RegistrationService : IRegistrationService
{
    private readonly ApplicationDbContext _context;

    public RegistrationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RegistrationResponse> RegisterForEventAsync(int eventId, int userId)
    {
        // Check if event exists
        var eventEntity = await _context.Events
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
        {
            throw new KeyNotFoundException($"Event with ID {eventId} not found");
        }

        // Validate event is active
        if (eventEntity.Status != EventStatus.Active)
        {
            throw new InvalidOperationException("Cannot register for a cancelled event");
        }

        // Validate event is in the future
        if (eventEntity.StartTime <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Cannot register for past events");
        }

        // Validate registration deadline
        if (eventEntity.RegistrationDeadline.HasValue && 
            eventEntity.RegistrationDeadline.Value <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Registration deadline has passed");
        }

        // Check if event is full
        var currentRegistrationCount = await _context.Registrations
            .CountAsync(r => r.EventId == eventId && r.Status == RegistrationStatus.Confirmed);

        if (currentRegistrationCount >= eventEntity.Capacity)
        {
            throw new InvalidOperationException("Event is already full");
        }

        // Check for duplicate registration
        var existingRegistration = await _context.Registrations
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

        if (existingRegistration != null)
        {
            if (existingRegistration.Status == RegistrationStatus.Confirmed)
            {
                throw new InvalidOperationException("User is already registered for this event");
            }
            
            // Reactivate cancelled registration
            existingRegistration.Status = RegistrationStatus.Confirmed;
            existingRegistration.RegisteredAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await MapToRegistrationResponse(existingRegistration);
        }

        // Check for time conflicts with other active registrations
        var eventEndTime = eventEntity.StartTime.AddMinutes(eventEntity.DurationMinutes);
        
        var hasTimeConflict = await _context.Registrations
            .Include(r => r.Event)
            .AnyAsync(r => 
                r.UserId == userId && 
                r.Status == RegistrationStatus.Confirmed &&
                r.Event!.Status == EventStatus.Active &&
                // Check if events overlap
                ((r.Event.StartTime < eventEndTime && 
                  r.Event.StartTime.AddMinutes(r.Event.DurationMinutes) > eventEntity.StartTime)));

        if (hasTimeConflict)
        {
            throw new InvalidOperationException("You have a time conflict with another registered event");
        }

        // Create new registration
        var registration = new Registration
        {
            EventId = eventId,
            UserId = userId,
            Status = RegistrationStatus.Confirmed,
            RegisteredAt = DateTime.UtcNow
        };

        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync();

        return await MapToRegistrationResponse(registration);
    }

    public async Task CancelRegistrationAsync(int eventId, int userId)
    {
        var registration = await _context.Registrations
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

        if (registration == null)
        {
            throw new KeyNotFoundException("Registration not found");
        }

        if (registration.Status == RegistrationStatus.Cancelled)
        {
            throw new InvalidOperationException("Registration is already cancelled");
        }

        // Set status to Cancelled (preserve history)
        registration.Status = RegistrationStatus.Cancelled;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<RegistrationResponse>> GetUserRegistrationsAsync(int userId)
    {
        var registrations = await _context.Registrations
            .Include(r => r.Event)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RegisteredAt)
            .ToListAsync();

        return registrations.Select(r => new RegistrationResponse
        {
            Id = r.Id,
            EventId = r.EventId,
            UserId = r.UserId,
            Status = r.Status,
            RegisteredAt = r.RegisteredAt,
            Event = new EventSummary
            {
                Id = r.Event!.Id,
                Title = r.Event.Title,
                Location = r.Event.Location,
                StartTime = r.Event.StartTime,
                DurationMinutes = r.Event.DurationMinutes,
                Status = r.Event.Status,
                ImageUrl = r.Event.ImageUrl
            }
        });
    }

    public async Task<IEnumerable<EventRegistrationResponse>> GetEventRegistrationsAsync(int eventId)
    {
        var registrations = await _context.Registrations
            .Include(r => r.User)
            .Where(r => r.EventId == eventId)
            .OrderByDescending(r => r.RegisteredAt)
            .ToListAsync();

        return registrations.Select(r => new EventRegistrationResponse
        {
            Id = r.Id,
            EventId = r.EventId,
            UserId = r.UserId,
            Status = r.Status,
            RegisteredAt = r.RegisteredAt,
            User = new UserSummary
            {
                Id = r.User!.Id,
                Name = r.User.Name,
                Email = r.User.Email,
                PhoneNumber = r.User.PhoneNumber
            }
        });
    }

    private async Task<RegistrationResponse> MapToRegistrationResponse(Registration registration)
    {
        // Reload with event data if needed
        if (registration.Event == null)
        {
            await _context.Entry(registration)
                .Reference(r => r.Event)
                .LoadAsync();
        }

        return new RegistrationResponse
        {
            Id = registration.Id,
            EventId = registration.EventId,
            UserId = registration.UserId,
            Status = registration.Status,
            RegisteredAt = registration.RegisteredAt,
            Event = new EventSummary
            {
                Id = registration.Event!.Id,
                Title = registration.Event.Title,
                Location = registration.Event.Location,
                StartTime = registration.Event.StartTime,
                DurationMinutes = registration.Event.DurationMinutes,
                Status = registration.Event.Status,
                ImageUrl = registration.Event.ImageUrl
            }
        };
    }
}
