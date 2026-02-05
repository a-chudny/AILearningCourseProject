namespace VolunteerPortal.API.Models.DTOs.Common;

/// <summary>
/// Standard API error response format for consistent error handling.
/// </summary>
public record ApiErrorResponse
{
    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public required string Message { get; init; }
    
    /// <summary>
    /// Machine-readable error code for client-side handling.
    /// </summary>
    public required string Code { get; init; }
    
    /// <summary>
    /// HTTP status code.
    /// </summary>
    public required int StatusCode { get; init; }
    
    /// <summary>
    /// Validation errors dictionary (field name -> error messages).
    /// Only populated for validation errors.
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; init; }
    
    /// <summary>
    /// Unique trace identifier for error tracking.
    /// </summary>
    public string? TraceId { get; init; }
    
    /// <summary>
    /// Timestamp when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
