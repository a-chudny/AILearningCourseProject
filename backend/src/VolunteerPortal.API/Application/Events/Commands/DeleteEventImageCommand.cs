using MediatR;
using VolunteerPortal.API.Models.DTOs.Events;

namespace VolunteerPortal.API.Application.Events.Commands;

/// <summary>
/// Command to delete an event's image.
/// </summary>
/// <param name="EventId">The ID of the event.</param>
/// <param name="UserId">ID of the user performing the deletion.</param>
/// <param name="UserRole">Role of the user performing the deletion.</param>
public sealed record DeleteEventImageCommand(
    int EventId,
    int UserId,
    string UserRole) : IRequest<EventResponse>;
