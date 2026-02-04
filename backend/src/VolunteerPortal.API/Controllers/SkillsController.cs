using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerPortal.API.Models.DTOs;
using VolunteerPortal.API.Models.DTOs.Skills;
using VolunteerPortal.API.Services.Interfaces;
using System.Security.Claims;

namespace VolunteerPortal.API.Controllers;

/// <summary>
/// Controller for managing skills and user skill associations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _skillService;

    public SkillsController(ISkillService skillService)
    {
        _skillService = skillService;
    }

    /// <summary>
    /// Get all available skills (public endpoint).
    /// </summary>
    /// <returns>List of all skills.</returns>
    /// <response code="200">Returns list of all skills.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SkillResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SkillResponse>>> GetAllSkills()
    {
        var skills = await _skillService.GetAllSkillsAsync();
        return Ok(skills);
    }

    /// <summary>
    /// Get current user's skills (authenticated endpoint).
    /// </summary>
    /// <returns>List of user's skills.</returns>
    /// <response code="200">Returns list of user's skills.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(List<SkillResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<SkillResponse>>> GetMySkills()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var skills = await _skillService.GetUserSkillsAsync(userId);
        return Ok(skills);
    }

    /// <summary>
    /// Update current user's skills (authenticated endpoint).
    /// Replaces all existing skills with the provided list.
    /// </summary>
    /// <param name="request">Request containing skill IDs to assign.</param>
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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID not found in token");
        }

        try
        {
            await _skillService.UpdateUserSkillsAsync(userId, request.SkillIds);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
