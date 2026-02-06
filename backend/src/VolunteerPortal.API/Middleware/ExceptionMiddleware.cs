using System.Text.Json;
using FluentValidation;
using VolunteerPortal.API.Exceptions;
using VolunteerPortal.API.Models.DTOs.Common;

namespace VolunteerPortal.API.Middleware;

/// <summary>
/// Global exception handling middleware for consistent API error responses.
/// </summary>
public class ExceptionMiddleware(
    RequestDelegate next, 
    ILogger<ExceptionMiddleware> logger,
    IHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex) { await HandleExceptionAsync(context, ex); }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var traceId = context.TraceIdentifier;
        var (statusCode, code, message, errors) = MapException(ex);
        
        LogException(ex, statusCode, traceId);
        
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        
        var response = new ApiErrorResponse
        {
            Message = message,
            Code = code,
            StatusCode = statusCode,
            TraceId = traceId,
            Errors = errors
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private (int Status, string Code, string Message, Dictionary<string, string[]>? Errors) MapException(Exception ex) => ex switch
    {
        // Custom app exceptions
        AppException appEx => (appEx.StatusCode, appEx.Code, appEx.Message, 
            appEx is Exceptions.ValidationException v ? v.Errors : null),
        
        // FluentValidation exceptions
        FluentValidation.ValidationException fv => (400, "VALIDATION_ERROR", 
            "One or more validation errors occurred.",
            fv.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => ToCamelCase(g.Key), g => g.Select(e => e.ErrorMessage).ToArray())),
        
        // Common service exceptions (backwards compatibility)
        KeyNotFoundException knf => (404, "NOT_FOUND", knf.Message, null),
        UnauthorizedAccessException ua => (403, "FORBIDDEN", ua.Message, null),
        InvalidOperationException io when io.Message.Contains("already") => (409, "CONFLICT", io.Message, null),
        InvalidOperationException io => (400, "BAD_REQUEST", io.Message, null),
        ArgumentException arg => (400, "BAD_REQUEST", arg.Message, null),
        
        // Cancellation
        OperationCanceledException => (499, "REQUEST_CANCELLED", "Request was cancelled.", null),
        
        // Unknown errors
        _ => (500, "INTERNAL_ERROR", 
            environment.IsDevelopment() ? $"{ex.GetType().Name}: {ex.Message}" : "An unexpected error occurred.", null)
    };

    private void LogException(Exception ex, int statusCode, string traceId) =>
        logger.Log(
            statusCode >= 500 ? LogLevel.Error : LogLevel.Warning,
            ex, "Request failed [{StatusCode}] TraceId: {TraceId}", statusCode, traceId);

    private static string ToCamelCase(string s) => string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s[1..];
}

/// <summary>
/// Extension methods for registering the exception middleware.
/// </summary>
public static class ExceptionMiddlewareExtensions
{
    /// <summary>
    /// Adds the global exception handling middleware to the pipeline.
    /// Should be added early in the pipeline to catch all exceptions.
    /// </summary>
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}
