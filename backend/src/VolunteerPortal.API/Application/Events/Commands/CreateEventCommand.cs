using MediatR;
using VolunteerPortal.API.Models.DTOs.Events;

namespace VolunteerPortal.API.Application.Events.Commands;

/// <summary>
/// Command to create a new event.
/// </summary>
/// <param name="Request">Event creation data.</param>
/// <param name="OrganizerId">ID of the user creating the event.</param>
public sealed record CreateEventCommand(
    CreateEventRequest Request,
    int OrganizerId) : IRequest<EventResponse>;
