using VolunteerPortal.API.Models.DTOs;

namespace VolunteerPortal.API.Models.DTOs.Auth;

/// <summary>
/// Response model for user profile information.
/// </summary>
public class UserResponse
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User's phone number (optional).
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// User's role (0 = Volunteer, 1 = Organizer, 2 = Admin).
    /// </summary>
    public int Role { get; set; }

    /// <summary>
    /// List of skills associated with the user.
    /// </summary>
    public List<SkillResponse> Skills { get; set; } = new();
}
