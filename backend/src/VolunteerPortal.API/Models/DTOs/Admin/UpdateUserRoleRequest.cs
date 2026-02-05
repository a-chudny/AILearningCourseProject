using System.ComponentModel.DataAnnotations;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Models.DTOs.Admin;

/// <summary>
/// Request DTO for updating a user's role
/// </summary>
public class UpdateUserRoleRequest
{
    /// <summary>
    /// The new role for the user (0 = Volunteer, 1 = Organizer, 2 = Admin)
    /// </summary>
    [Required]
    [Range(0, 2, ErrorMessage = "Role must be 0 (Volunteer), 1 (Organizer), or 2 (Admin)")]
    public int Role { get; set; }
}
