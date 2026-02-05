namespace VolunteerPortal.API.Models.DTOs.Admin;

/// <summary>
/// Response DTO for admin user list item
/// </summary>
public class AdminUserResponse
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User's full name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's role (0 = Volunteer, 1 = Organizer, 2 = Admin)
    /// </summary>
    public int Role { get; set; }

    /// <summary>
    /// User's role as string for display
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user is soft-deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the user account was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the user account was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
