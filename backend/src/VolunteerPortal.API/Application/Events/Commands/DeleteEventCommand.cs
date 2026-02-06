using MediatR;

namespace VolunteerPortal.API.Application.Events.Commands;

/// <summary>
/// Command to soft-delete an event.
/// </summary>
/// <param name="EventId">The ID of the event to delete.</param>
/// <param name="UserId">ID of the user performing the deletion.</param>
public sealed record DeleteEventCommand(int EventId, int UserId) : IRequest<Unit>;
