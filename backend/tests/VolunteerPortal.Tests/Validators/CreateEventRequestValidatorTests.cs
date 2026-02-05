using FluentValidation.TestHelper;
using VolunteerPortal.API.Models.DTOs.Events;
using VolunteerPortal.API.Validators;
using Xunit;

namespace VolunteerPortal.Tests.Validators;

/// <summary>
/// Unit tests for CreateEventRequestValidator
/// </summary>
public class CreateEventRequestValidatorTests
{
    private readonly CreateEventRequestValidator _validator;

    public CreateEventRequestValidatorTests()
    {
        _validator = new CreateEventRequestValidator();
    }

    #region Title Validation Tests

    [Fact]
    public void Validate_EmptyTitle_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Title = string.Empty;

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Validate_TitleTooLong_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Title = new string('a', 201);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 200 characters.");
    }

    [Fact]
    public void Validate_ValidTitle_HasNoError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Title = "Valid Event Title";

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public void Validate_EmptyDescription_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Description = string.Empty;

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description is required.");
    }

    [Fact]
    public void Validate_DescriptionTooLong_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Description = new string('a', 2001);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 2000 characters.");
    }

    #endregion

    #region Location Validation Tests

    [Fact]
    public void Validate_EmptyLocation_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Location = string.Empty;

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Location is required.");
    }

    [Fact]
    public void Validate_LocationTooLong_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Location = new string('a', 501);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Location cannot exceed 500 characters.");
    }

    #endregion

    #region StartTime Validation Tests

    [Fact]
    public void Validate_PastStartTime_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.StartTime = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartTime)
            .WithErrorMessage("Start time must be in the future.");
    }

    [Fact]
    public void Validate_FutureStartTime_HasNoError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.StartTime = DateTime.UtcNow.AddDays(7);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartTime);
    }

    #endregion

    #region DurationMinutes Validation Tests

    [Fact]
    public void Validate_ZeroDuration_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.DurationMinutes = 0;

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes)
            .WithErrorMessage("Duration must be at least 1 minute.");
    }

    [Fact]
    public void Validate_NegativeDuration_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.DurationMinutes = -10;

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void Validate_DurationTooLong_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.DurationMinutes = 1441; // More than 24 hours

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes)
            .WithErrorMessage("Duration cannot exceed 24 hours (1440 minutes).");
    }

    [Fact]
    public void Validate_ValidDuration_HasNoError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.DurationMinutes = 120;

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DurationMinutes);
    }

    #endregion

    #region Capacity Validation Tests

    [Fact]
    public void Validate_ZeroCapacity_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Capacity = 0;

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Capacity)
            .WithErrorMessage("Capacity must be at least 1.");
    }

    [Fact]
    public void Validate_NegativeCapacity_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Capacity = -5;

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Capacity);
    }

    [Fact]
    public void Validate_CapacityTooLarge_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Capacity = 10001;

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Capacity)
            .WithErrorMessage("Capacity cannot exceed 10,000.");
    }

    [Fact]
    public void Validate_ValidCapacity_HasNoError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Capacity = 100;

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Capacity);
    }

    #endregion

    #region ImageUrl Validation Tests

    [Fact]
    public void Validate_InvalidImageUrl_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.ImageUrl = "not-a-valid-url";

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageUrl)
            .WithErrorMessage("Image URL must be a valid URL.");
    }

    [Fact]
    public void Validate_ValidHttpImageUrl_HasNoError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.ImageUrl = "http://example.com/image.jpg";

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public void Validate_ValidHttpsImageUrl_HasNoError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.ImageUrl = "https://example.com/image.jpg";

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public void Validate_NullImageUrl_HasNoError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.ImageUrl = null;

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public void Validate_ImageUrlTooLong_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.ImageUrl = "https://example.com/" + new string('a', 500);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageUrl)
            .WithErrorMessage("Image URL cannot exceed 500 characters.");
    }

    #endregion

    #region RegistrationDeadline Validation Tests

    [Fact]
    public void Validate_DeadlineAfterStartTime_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.StartTime = DateTime.UtcNow.AddDays(7);
        model.RegistrationDeadline = DateTime.UtcNow.AddDays(10);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegistrationDeadline)
            .WithErrorMessage("Registration deadline must be before the event start time.");
    }

    [Fact]
    public void Validate_DeadlineBeforeStartTime_HasNoError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.StartTime = DateTime.UtcNow.AddDays(7);
        model.RegistrationDeadline = DateTime.UtcNow.AddDays(5);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RegistrationDeadline);
    }

    [Fact]
    public void Validate_NullDeadline_HasNoError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.RegistrationDeadline = null;

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RegistrationDeadline);
    }

    #endregion

    #region RequiredSkillIds Validation Tests

    [Fact]
    public void Validate_DuplicateSkillIds_HasError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.RequiredSkillIds = new List<int> { 1, 2, 1 };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RequiredSkillIds)
            .WithErrorMessage("Skill IDs must be unique.");
    }

    [Fact]
    public void Validate_UniqueSkillIds_HasNoError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.RequiredSkillIds = new List<int> { 1, 2, 3 };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredSkillIds);
    }

    [Fact]
    public void Validate_NullSkillIds_HasNoError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.RequiredSkillIds = null;

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredSkillIds);
    }

    [Fact]
    public void Validate_EmptySkillIds_HasNoError()
    {
        // Arrange
        var model = CreateValidRequest();
        model.RequiredSkillIds = new List<int>();

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredSkillIds);
    }

    #endregion

    #region Complete Request Validation Tests

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        // Arrange
        var model = CreateValidRequest();

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InvalidRequest_HasMultipleErrors()
    {
        // Arrange
        var model = new CreateEventRequest
        {
            Title = string.Empty,
            Description = string.Empty,
            Location = string.Empty,
            StartTime = DateTime.UtcNow.AddDays(-1),
            DurationMinutes = 0,
            Capacity = 0
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Description);
        result.ShouldHaveValidationErrorFor(x => x.Location);
        result.ShouldHaveValidationErrorFor(x => x.StartTime);
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
        result.ShouldHaveValidationErrorFor(x => x.Capacity);
    }

    #endregion

    #region Helper Methods

    private static CreateEventRequest CreateValidRequest()
    {
        return new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 120,
            Capacity = 50
        };
    }

    #endregion
}
