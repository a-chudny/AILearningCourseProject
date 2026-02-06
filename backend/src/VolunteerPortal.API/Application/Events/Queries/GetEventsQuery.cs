using MediatR;
using VolunteerPortal.API.Models.DTOs.Events;

namespace VolunteerPortal.API.Application.Events.Queries;

/// <summary>
/// Query to get a paginated list of events.
/// </summary>
/// <param name="QueryParams">Query parameters for filtering, sorting, and pagination.</param>
/// <param name="CurrentUserId">Optional user ID for skill-based filtering.</param>
public sealed record GetEventsQuery(
    EventQueryParams QueryParams,
    int? CurrentUserId = null) : IRequest<EventListResponse>;
