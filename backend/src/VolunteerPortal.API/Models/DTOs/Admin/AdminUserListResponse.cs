namespace VolunteerPortal.API.Models.DTOs.Admin;

/// <summary>
/// Paginated response for admin user list
/// </summary>
public class AdminUserListResponse
{
    /// <summary>
    /// List of users on the current page
    /// </summary>
    public List<AdminUserResponse> Users { get; set; } = new();

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of users matching the query
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage { get; set; }
}
