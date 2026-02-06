using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunteerPortal.API.Application.Skills.Commands;
using VolunteerPortal.API.Application.Skills.Queries;
using VolunteerPortal.API.Models.DTOs;
using VolunteerPortal.API.Models.DTOs.Skills;

namespace VolunteerPortal.API.Controllers;

/// <summary>
/// Controller for managing skills and user skill associations.
/// Provides endpoints for viewing available skills and managing user skill profiles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SkillsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillsController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR mediator for dispatching commands and queries.</param>
    public SkillsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all available skills.
    /// </summary>
    /// <remarks>
    /// Returns all skills in the system that can be assigned to users or required for events.
    /// This is a public endpoint accessible without authentication.
    /// </remarks>
    /// <returns>List of all available skills.</returns>
    /// <response code="200">Returns the list of all skills.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SkillResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SkillResponse>>> GetAllSkills()
    {
        var query = new GetAllSkillsQuery();
        var skills = await _mediator.Send(query);
        return Ok(skills);
    }

    /// <summary>
    /// Get current user's skills.
    /// </summary>
    /// <remarks>
    /// Returns all skills associated with the authenticated user's profile.
    /// </remarks>
    /// <returns>List of user's assigned skills.</returns>
    /// <response code="200">Returns the user's skills.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(List<SkillResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<SkillResponse>>> GetMySkills()
    {
        var userId = GetCurrentUserId();
        var query = new GetUserSkillsQuery(userId);
        var skills = await _mediator.Send(query);
        return Ok(skills);
    }

    /// <summary>
    /// Update current user's skills.
    /// </summary>
    /// <remarks>
    /// Replaces all existing skills with the provided list of skill IDs.
    /// To remove all skills, provide an empty array.
    /// Invalid skill IDs are ignored.
    /// </remarks>
    /// <param name="request">Request containing the skill IDs to assign to the user.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Skills updated successfully.</response>
    /// <response code="400">Invalid skill IDs provided.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMySkills([FromBody] UpdateUserSkillsRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateUserSkillsCommand(userId, request.SkillIds);
        await _mediator.Send(command);
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token");
        return userId;
    }
}
