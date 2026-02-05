using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Application.Admin.Models;

/// <summary>
/// Application layer model for admin user data.
/// </summary>
public class AdminUserModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Application layer model for admin statistics.
/// </summary>
public class AdminStatsModel
{
    public int TotalUsers { get; set; }
    public int TotalEvents { get; set; }
    public int TotalRegistrations { get; set; }
    public int RegistrationsThisMonth { get; set; }
    public int UpcomingEvents { get; set; }
}
