using MediatR;
using Microsoft.EntityFrameworkCore;
using VolunteerPortal.API.Application.Admin.Commands;
using VolunteerPortal.API.Application.Admin.Models;
using VolunteerPortal.API.Application.Admin.Queries;
using VolunteerPortal.API.Application.Common;
using VolunteerPortal.API.Data;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Application.Admin.Handlers;

/// <summary>
/// Handler for admin statistics query.
/// </summary>
public sealed class GetAdminStatsHandler : IRequestHandler<GetAdminStatsQuery, AdminStatsModel>
{
    private readonly ApplicationDbContext _context;

    public GetAdminStatsHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminStatsModel> Handle(GetAdminStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = firstDayOfMonth.AddMonths(1);

        return new AdminStatsModel
        {
            TotalUsers = await _context.Users
                .Where(u => !u.IsDeleted)
                .CountAsync(cancellationToken),

            TotalEvents = await _context.Events
                .Where(e => !e.IsDeleted)
                .CountAsync(cancellationToken),

            TotalRegistrations = await _context.Registrations
                .CountAsync(cancellationToken),

            RegistrationsThisMonth = await _context.Registrations
                .Where(r => r.RegisteredAt >= firstDayOfMonth && r.RegisteredAt < endOfMonth)
                .CountAsync(cancellationToken),

            UpcomingEvents = await _context.Events
                .Where(e => e.StartTime > now && e.Status == EventStatus.Active && !e.IsDeleted)
                .CountAsync(cancellationToken)
        };
    }
}

/// <summary>
/// Handler for getting paginated users list.
/// </summary>
public sealed class GetUsersHandler : IRequestHandler<GetUsersQuery, PagedResult<AdminUserModel>>
{
    private readonly ApplicationDbContext _context;

    public GetUsersHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminUserModel>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        // Start with IgnoreQueryFilters to allow access to soft-deleted users when needed
        var query = _context.Users.IgnoreQueryFilters().AsQueryable();

        // Filter by status
        query = request.Status?.ToLowerInvariant() switch
        {
            "active" => query.Where(u => !u.IsDeleted),
            "deleted" => query.Where(u => u.IsDeleted),
            _ when !request.IncludeDeleted => query.Where(u => !u.IsDeleted),
            _ => query
        };

        // Search by name or email
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(u => 
                u.Name.ToLower().Contains(searchLower) || 
                u.Email.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserModel
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                IsDeleted = u.IsDeleted,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminUserModel>
        {
            Items = users,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}

/// <summary>
/// Handler for updating user role command.
/// </summary>
public sealed class UpdateUserRoleHandler : IRequestHandler<UpdateUserRoleCommand, AdminUserModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateUserRoleHandler> _logger;

    public UpdateUserRoleHandler(ApplicationDbContext context, ILogger<UpdateUserRoleHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AdminUserModel> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == request.CurrentUserId)
            throw new InvalidOperationException("You cannot change your own role");

        // Use IgnoreQueryFilters to find soft-deleted users and return proper error message
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found");

        if (user.IsDeleted)
            throw new InvalidOperationException("Cannot modify a deleted user");

        var oldRole = user.Role;
        user.Role = request.NewRole;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminId} changed user {UserId} role from {OldRole} to {NewRole}",
            request.CurrentUserId, request.UserId, oldRole, request.NewRole);

        return new AdminUserModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsDeleted = user.IsDeleted,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for delete user command.
/// </summary>
public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand, DeleteUserResult>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeleteUserHandler> _logger;

    public DeleteUserHandler(ApplicationDbContext context, ILogger<DeleteUserHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DeleteUserResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == request.CurrentUserId)
            throw new InvalidOperationException("You cannot delete your own account");

        // Use IgnoreQueryFilters to find soft-deleted users and return proper error message
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found");

        if (user.IsDeleted)
            throw new InvalidOperationException("User is already deleted");

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminId} soft-deleted user {UserId} ({UserEmail})",
            request.CurrentUserId, request.UserId, user.Email);

        return new DeleteUserResult($"User {user.Name} has been deleted successfully");
    }
}
