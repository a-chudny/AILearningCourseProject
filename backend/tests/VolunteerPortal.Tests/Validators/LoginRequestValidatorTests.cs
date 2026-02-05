using FluentValidation.TestHelper;
using VolunteerPortal.API.Models.DTOs.Auth;
using VolunteerPortal.API.Validators;
using Xunit;

namespace VolunteerPortal.Tests.Validators;

/// <summary>
/// Unit tests for LoginRequestValidator
/// </summary>
public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;

    public LoginRequestValidatorTests()
    {
        _validator = new LoginRequestValidator();
    }

    #region Email Validation Tests

    [Fact]
    public void Validate_EmptyEmail_HasError()
    {
        // Arrange
        var model = new LoginRequest { Email = string.Empty, Password = "Password123" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Fact]
    public void Validate_InvalidEmailFormat_HasError()
    {
        // Arrange
        var model = new LoginRequest { Email = "not-an-email", Password = "Password123" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void Validate_EmailTooLong_HasError()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@test.com";
        var model = new LoginRequest { Email = longEmail, Password = "Password123" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email cannot exceed 255 characters");
    }

    [Fact]
    public void Validate_ValidEmail_HasNoError()
    {
        // Arrange
        var model = new LoginRequest { Email = "test@example.com", Password = "Password123" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Password Validation Tests

    [Fact]
    public void Validate_EmptyPassword_HasError()
    {
        // Arrange
        var model = new LoginRequest { Email = "test@example.com", Password = string.Empty };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    [Fact]
    public void Validate_NullPassword_HasError()
    {
        // Arrange
        var model = new LoginRequest { Email = "test@example.com", Password = null! };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_ValidPassword_HasNoError()
    {
        // Arrange - Login doesn't validate password complexity, just that it's not empty
        var model = new LoginRequest { Email = "test@example.com", Password = "any" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region Complete Request Validation Tests

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        // Arrange
        var model = new LoginRequest { Email = "test@example.com", Password = "Password123" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_BothFieldsEmpty_HasMultipleErrors()
    {
        // Arrange
        var model = new LoginRequest { Email = string.Empty, Password = string.Empty };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    #endregion
}
