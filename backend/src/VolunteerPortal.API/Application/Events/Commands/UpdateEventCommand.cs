using MediatR;
using VolunteerPortal.API.Models.DTOs.Events;

namespace VolunteerPortal.API.Application.Events.Commands;

/// <summary>
/// Command to update an existing event.
/// </summary>
/// <param name="EventId">The ID of the event to update.</param>
/// <param name="Request">Updated event data.</param>
/// <param name="UserId">ID of the user performing the update.</param>
public sealed record UpdateEventCommand(
    int EventId,
    UpdateEventRequest Request,
    int UserId) : IRequest<EventResponse>;
