using FluentValidation.TestHelper;
using VolunteerPortal.API.Models.DTOs.Auth;
using VolunteerPortal.API.Validators;
using Xunit;

namespace VolunteerPortal.Tests.Validators;

/// <summary>
/// Unit tests for RegisterRequestValidator
/// </summary>
public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    #region Email Validation Tests

    [Fact]
    public void Validate_EmptyEmail_HasError()
    {
        // Arrange
        var model = new RegisterRequest { Email = string.Empty, Password = "Password123", Name = "Test User" };

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
        var model = new RegisterRequest { Email = "not-an-email", Password = "Password123", Name = "Test User" };

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
        var model = new RegisterRequest { Email = longEmail, Password = "Password123", Name = "Test User" };

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
        var model = new RegisterRequest { Email = "test@example.com", Password = "Password123", Name = "Test User" };

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
        var model = new RegisterRequest { Email = "test@example.com", Password = string.Empty, Name = "Test User" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    [Fact]
    public void Validate_PasswordTooShort_HasError()
    {
        // Arrange
        var model = new RegisterRequest { Email = "test@example.com", Password = "Pass1", Name = "Test User" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters");
    }

    [Fact]
    public void Validate_PasswordWithoutNumber_HasError()
    {
        // Arrange
        var model = new RegisterRequest { Email = "test@example.com", Password = "PasswordNoNumber", Name = "Test User" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one number");
    }

    [Fact]
    public void Validate_ValidPassword_HasNoError()
    {
        // Arrange
        var model = new RegisterRequest { Email = "test@example.com", Password = "ValidPassword123", Name = "Test User" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public void Validate_EmptyName_HasError()
    {
        // Arrange
        var model = new RegisterRequest { Email = "test@example.com", Password = "Password123", Name = string.Empty };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Validate_NameTooLong_HasError()
    {
        // Arrange
        var longName = new string('a', 101);
        var model = new RegisterRequest { Email = "test@example.com", Password = "Password123", Name = longName };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_ValidName_HasNoError()
    {
        // Arrange
        var model = new RegisterRequest { Email = "test@example.com", Password = "Password123", Name = "Valid Name" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region PhoneNumber Validation Tests

    [Fact]
    public void Validate_PhoneNumberTooLong_HasError()
    {
        // Arrange
        var longPhone = new string('1', 21);
        var model = new RegisterRequest 
        { 
            Email = "test@example.com", 
            Password = "Password123", 
            Name = "Test User",
            PhoneNumber = longPhone 
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage("Phone number cannot exceed 20 characters");
    }

    [Fact]
    public void Validate_ValidPhoneNumber_HasNoError()
    {
        // Arrange
        var model = new RegisterRequest 
        { 
            Email = "test@example.com", 
            Password = "Password123", 
            Name = "Test User",
            PhoneNumber = "+1234567890" 
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validate_NullPhoneNumber_HasNoError()
    {
        // Arrange
        var model = new RegisterRequest 
        { 
            Email = "test@example.com", 
            Password = "Password123", 
            Name = "Test User",
            PhoneNumber = null 
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    #endregion

    #region Complete Request Validation Tests

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        // Arrange
        var model = new RegisterRequest 
        { 
            Email = "test@example.com", 
            Password = "Password123", 
            Name = "Test User",
            PhoneNumber = "+1234567890" 
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InvalidRequest_HasMultipleErrors()
    {
        // Arrange
        var model = new RegisterRequest 
        { 
            Email = string.Empty, 
            Password = string.Empty, 
            Name = string.Empty 
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    #endregion
}
