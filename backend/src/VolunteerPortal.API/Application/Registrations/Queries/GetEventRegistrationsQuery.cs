using MediatR;
using VolunteerPortal.API.Models.DTOs.Registrations;

namespace VolunteerPortal.API.Application.Registrations.Queries;

/// <summary>
/// Query to get all registrations for a specific event.
/// </summary>
/// <param name="EventId">The ID of the event.</param>
public sealed record GetEventRegistrationsQuery(int EventId) : IRequest<IEnumerable<EventRegistrationResponse>>;
