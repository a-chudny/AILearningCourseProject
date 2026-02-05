using MediatR;
using VolunteerPortal.API.Models.DTOs.Registrations;

namespace VolunteerPortal.API.Application.Registrations.Commands;

/// <summary>
/// Command to register a user for an event.
/// </summary>
/// <param name="EventId">The ID of the event to register for.</param>
/// <param name="UserId">The ID of the user registering.</param>
public sealed record RegisterForEventCommand(int EventId, int UserId) : IRequest<RegistrationResponse>;
