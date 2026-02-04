namespace VolunteerPortal.API.Models.DTOs.Events;

/// <summary>
/// Query parameters for filtering and paginating event list.
/// </summary>
public class EventQueryParams
{
    /// <summary>
    /// Page number (1-based). Default is 1.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page. Default is 20.
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// When true, includes past events. Default is false (upcoming only).
    /// </summary>
    public bool IncludePastEvents { get; set; } = false;

    /// <summary>
    /// When true, includes soft-deleted events (admin only). Default is false.
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// Optional search term to filter by event title or description.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Optional filter by event status (Active or Cancelled).
    /// </summary>
    public VolunteerPortal.API.Models.Enums.EventStatus? Status { get; set; }

    /// <summary>
    /// Sort order: StartTime, Title, CreatedAt. Default is StartTime ascending.
    /// </summary>
    public string SortBy { get; set; } = "StartTime";

    /// <summary>
    /// Sort direction: asc or desc. Default is asc.
    /// </summary>
    public string SortDirection { get; set; } = "asc";
}
