using MediatR;
using VolunteerPortal.API.Models.DTOs.Events;

namespace VolunteerPortal.API.Application.Events.Queries;

/// <summary>
/// Query to get a single event by ID.
/// </summary>
/// <param name="EventId">The ID of the event to retrieve.</param>
public sealed record GetEventByIdQuery(int EventId) : IRequest<EventResponse?>;
