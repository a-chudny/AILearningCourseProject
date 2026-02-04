namespace VolunteerPortal.API.Models.DTOs.Events;

/// <summary>
/// Paginated response containing a list of events.
/// </summary>
public class EventListResponse
{
    /// <summary>
    /// List of events in the current page.
    /// </summary>
    public List<EventResponse> Events { get; set; } = new();

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indicates whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Indicates whether there is a next page.
    /// </summary>
    public bool HasNextPage { get; set; }
}
