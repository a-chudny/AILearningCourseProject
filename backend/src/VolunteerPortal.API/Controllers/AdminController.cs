using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.DTOs.Admin;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Controllers;

/// <summary>
/// Controller for admin operations and statistics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ApplicationDbContext context,
        ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get admin dashboard statistics
    /// </summary>
    /// <returns>Admin statistics including user counts, event counts, and registrations</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AdminStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminStatsResponse>> GetStats()
    {
        try
        {
            // Get first and last day of current month (UTC)
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Total users (excluding soft-deleted)
            var totalUsers = await _context.Users
                .Where(u => !u.IsDeleted)
                .CountAsync();

            // Total events (excluding soft-deleted)
            var totalEvents = await _context.Events
                .Where(e => !e.IsDeleted)
                .CountAsync();

            // Total registrations (all statuses)
            var totalRegistrations = await _context.Registrations
                .CountAsync();

            // Registrations this month
            var registrationsThisMonth = await _context.Registrations
                .Where(r => r.RegisteredAt >= firstDayOfMonth && r.RegisteredAt <= lastDayOfMonth.AddDays(1))
                .CountAsync();

            // Upcoming events (future, active, not deleted)
            var upcomingEvents = await _context.Events
                .Where(e => e.StartTime > now && e.Status == EventStatus.Active && !e.IsDeleted)
                .CountAsync();

            return Ok(new AdminStatsResponse
            {
                TotalUsers = totalUsers,
                TotalEvents = totalEvents,
                TotalRegistrations = totalRegistrations,
                RegistrationsThisMonth = registrationsThisMonth,
                UpcomingEvents = upcomingEvents
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin statistics");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving statistics" });
        }
    }

    /// <summary>
    /// Get paginated list of users for admin management
    /// </summary>
    /// <param name="page">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 50)</param>
    /// <param name="search">Search by name or email</param>
    /// <param name="includeDeleted">Include soft-deleted users (default: false)</param>
    /// <param name="status">Filter by status: 'active', 'deleted', or null for all</param>
    /// <returns>Paginated list of users</returns>
    [HttpGet("users")]
    [ProducesResponseType(typeof(AdminUserListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminUserListResponse>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool includeDeleted = true,
        [FromQuery] string? status = null)
    {
        try
        {
            // Validate pagination parameters
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            // Build query
            var query = _context.Users.AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(u => !u.IsDeleted);
                }
                else if (status.Equals("deleted", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(u => u.IsDeleted);
                }
            }
            else if (!includeDeleted)
            {
                query = query.Where(u => !u.IsDeleted);
            }

            // Search by name or email
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(u => 
                    u.Name.ToLower().Contains(searchLower) || 
                    u.Email.ToLower().Contains(searchLower));
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Sort by created date (newest first) and paginate
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminUserResponse
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = (int)u.Role,
                    RoleName = u.Role.ToString(),
                    IsDeleted = u.IsDeleted,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync();

            return Ok(new AdminUserListResponse
            {
                Users = users,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users list");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Update a user's role
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">New role information</param>
    /// <returns>Updated user information</returns>
    [HttpPut("users/{id}/role")]
    [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserResponse>> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            // Get current user ID from claims
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return Unauthorized(new { message = "Unable to identify current user" });
            }

            // Cannot change own role
            if (id == currentUserId)
            {
                return BadRequest(new { message = "You cannot change your own role" });
            }

            // Find the user
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Cannot modify soft-deleted users
            if (user.IsDeleted)
            {
                return BadRequest(new { message = "Cannot modify a deleted user" });
            }

            // Update role
            var oldRole = user.Role;
            user.Role = (UserRole)request.Role;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} changed user {UserId} role from {OldRole} to {NewRole}",
                currentUserId, id, oldRole, user.Role);

            return Ok(new AdminUserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = (int)user.Role,
                RoleName = user.Role.ToString(),
                IsDeleted = user.IsDeleted,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role for user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating user role" });
        }
    }

    /// <summary>
    /// Soft delete a user
    /// </summary>
    /// <param name="id">User ID to delete</param>
    /// <returns>Success message</returns>
    [HttpDelete("users/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            // Get current user ID from claims
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return Unauthorized(new { message = "Unable to identify current user" });
            }

            // Cannot delete self
            if (id == currentUserId)
            {
                return BadRequest(new { message = "You cannot delete your own account" });
            }

            // Find the user
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Check if already deleted
            if (user.IsDeleted)
            {
                return BadRequest(new { message = "User is already deleted" });
            }

            // Soft delete
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} soft-deleted user {UserId} ({UserEmail})",
                currentUserId, id, user.Email);

            return Ok(new { message = $"User {user.Name} has been deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while deleting the user" });
        }
    }
}
