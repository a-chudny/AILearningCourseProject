namespace VolunteerPortal.API.Models.DTOs.Auth;

/// <summary>
/// Response model for authentication operations (register/login)
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// User's unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's full name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User's role (0=Volunteer, 1=Organizer, 2=Admin)
    /// </summary>
    public int Role { get; set; }

    /// <summary>
    /// JWT authentication token
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
