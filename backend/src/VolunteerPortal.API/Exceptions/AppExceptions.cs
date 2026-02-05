namespace VolunteerPortal.API.Exceptions;

/// <summary>
/// Base exception for application-specific errors with standardized error codes.
/// </summary>
public abstract class AppException : Exception
{
    /// <summary>
    /// Machine-readable error code.
    /// </summary>
    public string Code { get; }
    
    /// <summary>
    /// HTTP status code to return.
    /// </summary>
    public abstract int StatusCode { get; }

    protected AppException(string message, string code) : base(message)
    {
        Code = code;
    }

    protected AppException(string message, string code, Exception innerException) 
        : base(message, innerException)
    {
        Code = code;
    }
}

/// <summary>
/// Exception thrown when a requested resource is not found (HTTP 404).
/// </summary>
public class NotFoundException : AppException
{
    public override int StatusCode => 404;

    public NotFoundException(string message) 
        : base(message, "NOT_FOUND")
    {
    }

    public NotFoundException(string resourceName, object id) 
        : base($"{resourceName} with ID '{id}' was not found.", "NOT_FOUND")
    {
    }
}

/// <summary>
/// Exception thrown when request validation fails (HTTP 400).
/// </summary>
public class ValidationException : AppException
{
    public override int StatusCode => 400;
    
    /// <summary>
    /// Validation errors dictionary (field name -> error messages).
    /// </summary>
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message) 
        : base(message, "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, Dictionary<string, string[]> errors) 
        : base(message, "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    public ValidationException(string field, string errorMessage) 
        : base($"Validation failed for field '{field}'.", "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, [errorMessage] }
        };
    }
}

/// <summary>
/// Exception thrown when user is not authenticated (HTTP 401).
/// </summary>
public class UnauthorizedException : AppException
{
    public override int StatusCode => 401;

    public UnauthorizedException() 
        : base("Authentication is required to access this resource.", "UNAUTHORIZED")
    {
    }

    public UnauthorizedException(string message) 
        : base(message, "UNAUTHORIZED")
    {
    }
}

/// <summary>
/// Exception thrown when user lacks permission (HTTP 403).
/// </summary>
public class ForbiddenException : AppException
{
    public override int StatusCode => 403;

    public ForbiddenException() 
        : base("You do not have permission to access this resource.", "FORBIDDEN")
    {
    }

    public ForbiddenException(string message) 
        : base(message, "FORBIDDEN")
    {
    }
}

/// <summary>
/// Exception thrown when a conflict occurs (HTTP 409).
/// </summary>
public class ConflictException : AppException
{
    public override int StatusCode => 409;

    public ConflictException(string message) 
        : base(message, "CONFLICT")
    {
    }

    public ConflictException(string resourceName, string conflictReason) 
        : base($"{resourceName} conflict: {conflictReason}", "CONFLICT")
    {
    }
}

/// <summary>
/// Exception thrown when business rule is violated (HTTP 422).
/// </summary>
public class BusinessRuleException : AppException
{
    public override int StatusCode => 422;

    public BusinessRuleException(string message) 
        : base(message, "BUSINESS_RULE_VIOLATION")
    {
    }

    public BusinessRuleException(string message, string code) 
        : base(message, code)
    {
    }
}
