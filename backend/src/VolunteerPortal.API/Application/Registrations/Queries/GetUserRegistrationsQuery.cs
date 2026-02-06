using MediatR;
using VolunteerPortal.API.Models.DTOs.Registrations;

namespace VolunteerPortal.API.Application.Registrations.Queries;

/// <summary>
/// Query to get all registrations for the current user.
/// </summary>
/// <param name="UserId">The ID of the user.</param>
public sealed record GetUserRegistrationsQuery(int UserId) : IRequest<IEnumerable<RegistrationResponse>>;
