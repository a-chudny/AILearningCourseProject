using MediatR;
using VolunteerPortal.API.Application.Skills.Commands;
using VolunteerPortal.API.Application.Skills.Queries;
using VolunteerPortal.API.Models.DTOs;
using VolunteerPortal.API.Services.Interfaces;

namespace VolunteerPortal.API.Application.Skills.Handlers;

/// <summary>
/// Handler for getting all available skills.
/// </summary>
public sealed class GetAllSkillsHandler : IRequestHandler<GetAllSkillsQuery, List<SkillResponse>>
{
    private readonly ISkillService _skillService;

    public GetAllSkillsHandler(ISkillService skillService)
    {
        _skillService = skillService;
    }

    public async Task<List<SkillResponse>> Handle(GetAllSkillsQuery request, CancellationToken cancellationToken)
    {
        return await _skillService.GetAllSkillsAsync();
    }
}

/// <summary>
/// Handler for getting user's skills.
/// </summary>
public sealed class GetUserSkillsHandler : IRequestHandler<GetUserSkillsQuery, List<SkillResponse>>
{
    private readonly ISkillService _skillService;

    public GetUserSkillsHandler(ISkillService skillService)
    {
        _skillService = skillService;
    }

    public async Task<List<SkillResponse>> Handle(GetUserSkillsQuery request, CancellationToken cancellationToken)
    {
        return await _skillService.GetUserSkillsAsync(request.UserId);
    }
}

/// <summary>
/// Handler for updating user's skills.
/// </summary>
public sealed class UpdateUserSkillsHandler : IRequestHandler<UpdateUserSkillsCommand, Unit>
{
    private readonly ISkillService _skillService;

    public UpdateUserSkillsHandler(ISkillService skillService)
    {
        _skillService = skillService;
    }

    public async Task<Unit> Handle(UpdateUserSkillsCommand request, CancellationToken cancellationToken)
    {
        await _skillService.UpdateUserSkillsAsync(request.UserId, request.SkillIds);
        return Unit.Value;
    }
}
