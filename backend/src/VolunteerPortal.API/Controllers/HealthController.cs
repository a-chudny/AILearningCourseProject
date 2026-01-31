using Microsoft.AspNetCore.Mvc;

namespace VolunteerPortal.API.Controllers;

/// <summary>
/// Health check controller for monitoring API status.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Gets the health status of the API.
    /// </summary>
    /// <returns>Health status information.</returns>
    /// <response code="200">API is healthy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0"
        });
    }
}

/// <summary>
/// Health check response model.
/// </summary>
public record HealthResponse
{
    /// <summary>
    /// Current health status.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Timestamp of the health check.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// API version.
    /// </summary>
    public required string Version { get; init; }
}
