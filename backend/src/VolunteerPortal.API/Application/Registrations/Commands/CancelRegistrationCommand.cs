using MediatR;

namespace VolunteerPortal.API.Application.Registrations.Commands;

/// <summary>
/// Command to cancel a user's registration for an event.
/// </summary>
/// <param name="EventId">The ID of the event to cancel registration for.</param>
/// <param name="UserId">The ID of the user cancelling the registration.</param>
public sealed record CancelRegistrationCommand(int EventId, int UserId) : IRequest<Unit>;
