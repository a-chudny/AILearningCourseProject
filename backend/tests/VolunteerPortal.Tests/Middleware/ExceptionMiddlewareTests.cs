using System.Net;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using VolunteerPortal.API.Exceptions;
using VolunteerPortal.API.Middleware;
using VolunteerPortal.API.Models.DTOs.Common;

namespace VolunteerPortal.Tests.Middleware;

public class ExceptionMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionMiddleware>> _loggerMock;
    private readonly Mock<IHostEnvironment> _environmentMock;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExceptionMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
        _environmentMock = new Mock<IHostEnvironment>();
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    private ExceptionMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new ExceptionMiddleware(next, _loggerMock.Object, _environmentMock.Object);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "test-trace-id";
        return context;
    }

    private async Task<ApiErrorResponse?> GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ApiErrorResponse>(body, _jsonOptions);
    }

    [Fact]
    public async Task InvokeAsync_NoException_PassesThrough()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_NotFoundException_Returns404()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => throw new NotFoundException("Event", 123));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(404, context.Response.StatusCode);
        var response = await GetResponseBody(context);
        Assert.NotNull(response);
        Assert.Equal("NOT_FOUND", response.Code);
        Assert.Contains("123", response.Message);
        Assert.Equal("test-trace-id", response.TraceId);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_Returns400WithErrors()
    {
        // Arrange
        var context = CreateHttpContext();
        var errors = new Dictionary<string, string[]>
        {
            { "email", ["Email is required", "Email is invalid"] },
            { "password", ["Password must be at least 8 characters"] }
        };
        var middleware = CreateMiddleware(_ => 
            throw new VolunteerPortal.API.Exceptions.ValidationException("Validation failed", errors));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(400, context.Response.StatusCode);
        var response = await GetResponseBody(context);
        Assert.NotNull(response);
        Assert.Equal("VALIDATION_ERROR", response.Code);
        Assert.NotNull(response.Errors);
        Assert.Equal(2, response.Errors.Count);
        Assert.Contains("email", response.Errors.Keys);
    }

    [Fact]
    public async Task InvokeAsync_FluentValidationException_Returns400WithErrors()
    {
        // Arrange
        var context = CreateHttpContext();
        var failures = new List<ValidationFailure>
        {
            new("Email", "Email is required"),
            new("Email", "Email is invalid"),
            new("Password", "Password is required")
        };
        var middleware = CreateMiddleware(_ => 
            throw new FluentValidation.ValidationException(failures));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(400, context.Response.StatusCode);
        var response = await GetResponseBody(context);
        Assert.NotNull(response);
        Assert.Equal("VALIDATION_ERROR", response.Code);
        Assert.NotNull(response.Errors);
        Assert.Equal(2, response.Errors.Count); // email and password
        Assert.Equal(2, response.Errors["email"].Length); // 2 email errors
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedException_Returns401()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => throw new UnauthorizedException());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(401, context.Response.StatusCode);
        var response = await GetResponseBody(context);
        Assert.NotNull(response);
        Assert.Equal("UNAUTHORIZED", response.Code);
    }

    [Fact]
    public async Task InvokeAsync_ForbiddenException_Returns403()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => 
            throw new ForbiddenException("Only admins can perform this action"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(403, context.Response.StatusCode);
        var response = await GetResponseBody(context);
        Assert.NotNull(response);
        Assert.Equal("FORBIDDEN", response.Code);
        Assert.Contains("admins", response.Message);
    }

    [Fact]
    public async Task InvokeAsync_ConflictException_Returns409()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => 
            throw new ConflictException("User", "Email already exists"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(409, context.Response.StatusCode);
        var response = await GetResponseBody(context);
        Assert.NotNull(response);
        Assert.Equal("CONFLICT", response.Code);
    }

    [Fact]
    public async Task InvokeAsync_BusinessRuleException_Returns422()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => 
            throw new BusinessRuleException("Event is already at full capacity"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(422, context.Response.StatusCode);
        var response = await GetResponseBody(context);
        Assert.NotNull(response);
        Assert.Equal("BUSINESS_RULE_VIOLATION", response.Code);
    }

    [Fact]
    public async Task InvokeAsync_OperationCanceledException_Returns499()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => throw new OperationCanceledException());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(499, context.Response.StatusCode);
        var response = await GetResponseBody(context);
        Assert.NotNull(response);
        Assert.Equal("REQUEST_CANCELLED", response.Code);
    }

    [Fact]
    public async Task InvokeAsync_UnknownException_Returns500()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => 
            throw new NotSupportedException("Something unexpected happened"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(500, context.Response.StatusCode);
        var response = await GetResponseBody(context);
        Assert.NotNull(response);
        Assert.Equal("INTERNAL_ERROR", response.Code);
        Assert.Contains("NotSupportedException", response.Message); // In development mode
    }

    [Fact]
    public async Task InvokeAsync_UnknownException_Production_HidesDetails()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => 
            throw new NotSupportedException("Sensitive database error details"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(500, context.Response.StatusCode);
        var response = await GetResponseBody(context);
        Assert.NotNull(response);
        Assert.DoesNotContain("Sensitive", response.Message);
        Assert.Contains("unexpected error", response.Message);
    }

    [Fact]
    public async Task InvokeAsync_SetsContentTypeToJson()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => throw new NotFoundException("Resource"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_IncludesTimestamp()
    {
        // Arrange
        var context = CreateHttpContext();
        var beforeTest = DateTime.UtcNow;
        var middleware = CreateMiddleware(_ => throw new NotFoundException("Resource"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await GetResponseBody(context);
        Assert.NotNull(response);
        Assert.True(response.Timestamp >= beforeTest);
    }

    [Fact]
    public async Task InvokeAsync_LogsErrorForServerErrors()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => throw new Exception("Test error"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsWarningForClientErrors()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => throw new NotFoundException("Resource"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
