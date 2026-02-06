using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunteerPortal.API.Application.Admin.Commands;
using VolunteerPortal.API.Application.Admin.Queries;
using VolunteerPortal.API.Models.DTOs.Admin;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Controllers;

/// <summary>
/// Controller for admin operations and statistics.
/// Provides endpoints for dashboard statistics, user management, and administrative tasks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR mediator for dispatching commands and queries.</param>
    /// <param name="mapper">AutoMapper instance for DTO mapping.</param>
    public AdminController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Get admin dashboard statistics.
    /// </summary>
    /// <remarks>
    /// Returns aggregated statistics including:
    /// - Total active users count
    /// - Total events count
    /// - Total and monthly registration counts
    /// - Upcoming events count
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Admin dashboard statistics.</returns>
    /// <response code="200">Returns the admin statistics.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have admin role.</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AdminStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminStatsResponse>> GetStats(CancellationToken cancellationToken)
    {
        var query = new GetAdminStatsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(_mapper.Map<AdminStatsResponse>(result));
    }

    /// <summary>
    /// Get paginated list of users for admin management.
    /// </summary>
    /// <remarks>
    /// Supports filtering by status (active/deleted), search by name or email,
    /// and pagination with configurable page size (max 50 items per page).
    /// </remarks>
    /// <param name="page">Page number (1-based). Default: 1.</param>
    /// <param name="pageSize">Number of items per page. Default: 10, Max: 50.</param>
    /// <param name="search">Optional search term to filter by name or email.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted users. Default: true.</param>
    /// <param name="status">Filter by status: 'active', 'deleted', or null for all.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of users with metadata.</returns>
    /// <response code="200">Returns the paginated user list.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have admin role.</response>
    [HttpGet("users")]
    [ProducesResponseType(typeof(AdminUserListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminUserListResponse>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool includeDeleted = true,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersQuery(page, pageSize, search, includeDeleted, status);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(_mapper.Map<AdminUserListResponse>(result));
    }

    /// <summary>
    /// Update a user's role.
    /// </summary>
    /// <remarks>
    /// Allows changing a user's role between Volunteer, Organizer, and Admin.
    /// Admins cannot change their own role.
    /// Deleted users cannot be modified.
    /// </remarks>
    /// <param name="id">The ID of the user to update.</param>
    /// <param name="request">The new role information.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated user information.</returns>
    /// <response code="200">Returns the updated user.</response>
    /// <response code="400">Cannot change own role or modify deleted user.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have admin role.</response>
    /// <response code="404">User not found.</response>
    [HttpPut("users/{id}/role")]
    [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserResponse>> UpdateUserRole(
        int id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserRoleCommand(id, (UserRole)request.Role, GetCurrentUserId());
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(_mapper.Map<AdminUserResponse>(result));
    }

    /// <summary>
    /// Soft delete a user.
    /// </summary>
    /// <remarks>
    /// Performs a soft delete, marking the user as deleted without removing their data.
    /// Admins cannot delete their own account.
    /// Already deleted users cannot be deleted again.
    /// </remarks>
    /// <param name="id">The ID of the user to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success message confirming deletion.</returns>
    /// <response code="200">User successfully deleted.</response>
    /// <response code="400">Cannot delete own account or user already deleted.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have admin role.</response>
    /// <response code="404">User not found.</response>
    [HttpDelete("users/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand(id, GetCurrentUserId());
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(new { message = result.Message });
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("Unable to identify current user");
        return userId;
    }
}
