using MediatR;
using VolunteerPortal.API.Models.DTOs.Events;

namespace VolunteerPortal.API.Application.Events.Commands;

/// <summary>
/// Command to cancel an event.
/// </summary>
/// <param name="EventId">The ID of the event to cancel.</param>
/// <param name="UserId">ID of the user performing the cancellation.</param>
/// <param name="UserRole">Role of the user performing the cancellation.</param>
public sealed record CancelEventCommand(
    int EventId,
    int UserId,
    string UserRole) : IRequest<EventResponse>;
