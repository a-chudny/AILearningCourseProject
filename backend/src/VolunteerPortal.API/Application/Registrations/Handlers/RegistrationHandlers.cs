using MediatR;
using VolunteerPortal.API.Application.Registrations.Commands;
using VolunteerPortal.API.Application.Registrations.Queries;
using VolunteerPortal.API.Models.DTOs.Registrations;
using VolunteerPortal.API.Services.Interfaces;

namespace VolunteerPortal.API.Application.Registrations.Handlers;

/// <summary>
/// Handler for registering a user for an event.
/// </summary>
public sealed class RegisterForEventHandler : IRequestHandler<RegisterForEventCommand, RegistrationResponse>
{
    private readonly IRegistrationService _registrationService;

    public RegisterForEventHandler(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public async Task<RegistrationResponse> Handle(RegisterForEventCommand request, CancellationToken cancellationToken)
    {
        return await _registrationService.RegisterForEventAsync(request.EventId, request.UserId);
    }
}

/// <summary>
/// Handler for cancelling a user's event registration.
/// </summary>
public sealed class CancelRegistrationHandler : IRequestHandler<CancelRegistrationCommand, Unit>
{
    private readonly IRegistrationService _registrationService;

    public CancelRegistrationHandler(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public async Task<Unit> Handle(CancelRegistrationCommand request, CancellationToken cancellationToken)
    {
        await _registrationService.CancelRegistrationAsync(request.EventId, request.UserId);
        return Unit.Value;
    }
}

/// <summary>
/// Handler for getting user's registrations.
/// </summary>
public sealed class GetUserRegistrationsHandler : IRequestHandler<GetUserRegistrationsQuery, IEnumerable<RegistrationResponse>>
{
    private readonly IRegistrationService _registrationService;

    public GetUserRegistrationsHandler(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public async Task<IEnumerable<RegistrationResponse>> Handle(GetUserRegistrationsQuery request, CancellationToken cancellationToken)
    {
        return await _registrationService.GetUserRegistrationsAsync(request.UserId);
    }
}

/// <summary>
/// Handler for getting event registrations.
/// </summary>
public sealed class GetEventRegistrationsHandler : IRequestHandler<GetEventRegistrationsQuery, IEnumerable<EventRegistrationResponse>>
{
    private readonly IRegistrationService _registrationService;

    public GetEventRegistrationsHandler(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public async Task<IEnumerable<EventRegistrationResponse>> Handle(GetEventRegistrationsQuery request, CancellationToken cancellationToken)
    {
        return await _registrationService.GetEventRegistrationsAsync(request.EventId);
    }
}
