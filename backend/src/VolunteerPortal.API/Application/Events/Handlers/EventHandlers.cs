using MediatR;
using VolunteerPortal.API.Application.Events.Commands;
using VolunteerPortal.API.Application.Events.Queries;
using VolunteerPortal.API.Models.DTOs.Events;
using VolunteerPortal.API.Models.Enums;
using VolunteerPortal.API.Services.Interfaces;

namespace VolunteerPortal.API.Application.Events.Handlers;

/// <summary>
/// Handler for getting paginated list of events.
/// </summary>
public sealed class GetEventsHandler : IRequestHandler<GetEventsQuery, EventListResponse>
{
    private readonly IEventService _eventService;

    public GetEventsHandler(IEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task<EventListResponse> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        return await _eventService.GetAllAsync(request.QueryParams, request.CurrentUserId);
    }
}

/// <summary>
/// Handler for getting a single event by ID.
/// </summary>
public sealed class GetEventByIdHandler : IRequestHandler<GetEventByIdQuery, EventResponse?>
{
    private readonly IEventService _eventService;

    public GetEventByIdHandler(IEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task<EventResponse?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        return await _eventService.GetByIdAsync(request.EventId);
    }
}

/// <summary>
/// Handler for creating a new event.
/// </summary>
public sealed class CreateEventHandler : IRequestHandler<CreateEventCommand, EventResponse>
{
    private readonly IEventService _eventService;

    public CreateEventHandler(IEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task<EventResponse> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        return await _eventService.CreateAsync(request.Request, request.OrganizerId);
    }
}

/// <summary>
/// Handler for updating an existing event.
/// </summary>
public sealed class UpdateEventHandler : IRequestHandler<UpdateEventCommand, EventResponse>
{
    private readonly IEventService _eventService;

    public UpdateEventHandler(IEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task<EventResponse> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        return await _eventService.UpdateAsync(request.EventId, request.Request, request.UserId);
    }
}

/// <summary>
/// Handler for deleting an event.
/// </summary>
public sealed class DeleteEventHandler : IRequestHandler<DeleteEventCommand, Unit>
{
    private readonly IEventService _eventService;

    public DeleteEventHandler(IEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task<Unit> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        await _eventService.DeleteAsync(request.EventId, request.UserId);
        return Unit.Value;
    }
}

/// <summary>
/// Handler for uploading an event image.
/// </summary>
public sealed class UploadEventImageHandler : IRequestHandler<UploadEventImageCommand, EventResponse>
{
    private readonly IEventService _eventService;
    private readonly IFileStorageService _fileStorageService;

    public UploadEventImageHandler(IEventService eventService, IFileStorageService fileStorageService)
    {
        _eventService = eventService;
        _fileStorageService = fileStorageService;
    }

    public async Task<EventResponse> Handle(UploadEventImageCommand request, CancellationToken cancellationToken)
    {
        var existingEvent = await _eventService.GetByIdAsync(request.EventId)
            ?? throw new KeyNotFoundException($"Event with ID {request.EventId} not found.");

        ValidateEventOwnership(existingEvent, request.UserId, request.UserRole);

        // Delete existing image if present
        if (!string.IsNullOrEmpty(existingEvent.ImageUrl))
        {
            await _fileStorageService.DeleteAsync(existingEvent.ImageUrl);
        }

        // Upload new image
        var imageUrl = await _fileStorageService.UploadAsync(request.File, "events");

        // Update event with new image URL
        var updateRequest = CreateUpdateRequest(existingEvent, imageUrl: imageUrl);
        return await _eventService.UpdateAsync(request.EventId, updateRequest, request.UserId);
    }

    private static void ValidateEventOwnership(EventResponse evt, int userId, string userRole)
    {
        if (evt.OrganizerId != userId && userRole != "Admin")
        {
            throw new UnauthorizedAccessException("You don't have permission to modify this event.");
        }
    }

    private static UpdateEventRequest CreateUpdateRequest(EventResponse evt, string? imageUrl = null)
    {
        return new UpdateEventRequest
        {
            Title = evt.Title,
            Description = evt.Description,
            Location = evt.Location,
            StartTime = evt.StartTime,
            DurationMinutes = evt.DurationMinutes,
            Capacity = evt.Capacity,
            ImageUrl = imageUrl ?? evt.ImageUrl,
            RegistrationDeadline = evt.RegistrationDeadline,
            RequiredSkillIds = evt.RequiredSkills.Select(s => s.Id).ToList(),
            Status = evt.Status
        };
    }
}

/// <summary>
/// Handler for deleting an event image.
/// </summary>
public sealed class DeleteEventImageHandler : IRequestHandler<DeleteEventImageCommand, EventResponse>
{
    private readonly IEventService _eventService;
    private readonly IFileStorageService _fileStorageService;

    public DeleteEventImageHandler(IEventService eventService, IFileStorageService fileStorageService)
    {
        _eventService = eventService;
        _fileStorageService = fileStorageService;
    }

    public async Task<EventResponse> Handle(DeleteEventImageCommand request, CancellationToken cancellationToken)
    {
        var existingEvent = await _eventService.GetByIdAsync(request.EventId)
            ?? throw new KeyNotFoundException($"Event with ID {request.EventId} not found.");

        ValidateEventOwnership(existingEvent, request.UserId, request.UserRole);

        // Delete existing image if present
        if (!string.IsNullOrEmpty(existingEvent.ImageUrl))
        {
            await _fileStorageService.DeleteAsync(existingEvent.ImageUrl);
        }

        // Update event to remove image URL
        var updateRequest = CreateUpdateRequest(existingEvent, imageUrl: null);
        return await _eventService.UpdateAsync(request.EventId, updateRequest, request.UserId);
    }

    private static void ValidateEventOwnership(EventResponse evt, int userId, string userRole)
    {
        if (evt.OrganizerId != userId && userRole != "Admin")
        {
            throw new UnauthorizedAccessException("You don't have permission to modify this event.");
        }
    }

    private static UpdateEventRequest CreateUpdateRequest(EventResponse evt, string? imageUrl)
    {
        return new UpdateEventRequest
        {
            Title = evt.Title,
            Description = evt.Description,
            Location = evt.Location,
            StartTime = evt.StartTime,
            DurationMinutes = evt.DurationMinutes,
            Capacity = evt.Capacity,
            ImageUrl = imageUrl,
            RegistrationDeadline = evt.RegistrationDeadline,
            RequiredSkillIds = evt.RequiredSkills.Select(s => s.Id).ToList(),
            Status = evt.Status
        };
    }
}

/// <summary>
/// Handler for cancelling an event.
/// </summary>
public sealed class CancelEventHandler : IRequestHandler<CancelEventCommand, EventResponse>
{
    private readonly IEventService _eventService;

    public CancelEventHandler(IEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task<EventResponse> Handle(CancelEventCommand request, CancellationToken cancellationToken)
    {
        var existingEvent = await _eventService.GetByIdAsync(request.EventId)
            ?? throw new KeyNotFoundException($"Event with ID {request.EventId} not found.");

        ValidateEventOwnership(existingEvent, request.UserId, request.UserRole);

        if (existingEvent.Status != EventStatus.Active)
        {
            throw new InvalidOperationException("Only active events can be cancelled.");
        }

        var updateRequest = new UpdateEventRequest
        {
            Title = existingEvent.Title,
            Description = existingEvent.Description,
            Location = existingEvent.Location,
            StartTime = existingEvent.StartTime,
            DurationMinutes = existingEvent.DurationMinutes,
            Capacity = existingEvent.Capacity,
            ImageUrl = existingEvent.ImageUrl,
            RegistrationDeadline = existingEvent.RegistrationDeadline,
            RequiredSkillIds = existingEvent.RequiredSkills.Select(s => s.Id).ToList(),
            Status = EventStatus.Cancelled
        };

        return await _eventService.UpdateAsync(request.EventId, updateRequest, request.UserId);
    }

    private static void ValidateEventOwnership(EventResponse evt, int userId, string userRole)
    {
        if (evt.OrganizerId != userId && userRole != "Admin")
        {
            throw new UnauthorizedAccessException("You don't have permission to modify this event.");
        }
    }
}
