using FluentValidation.TestHelper;
using VolunteerPortal.API.Models.DTOs.Events;
using VolunteerPortal.API.Models.Enums;
using VolunteerPortal.API.Validators;
using Xunit;

namespace VolunteerPortal.Tests.Validators;

/// <summary>
/// Tests for UpdateEventRequestValidator.
/// </summary>
public class UpdateEventRequestValidatorTests
{
    private readonly UpdateEventRequestValidator _validator = new();

    private static UpdateEventRequest CreateValidRequest() => new()
    {
        Title = "Community Garden Project",
        Description = "Join us for a day of planting and community building.",
        Location = "Central Park",
        StartTime = DateTime.UtcNow.AddDays(7),
        DurationMinutes = 120,
        Capacity = 25,
        ImageUrl = null,
        RegistrationDeadline = DateTime.UtcNow.AddDays(5),
        Status = EventStatus.Active,
        RequiredSkillIds = null
    };

    #region Title Tests

    [Fact]
    public void Validate_EmptyTitle_HasError()
    {
        var model = CreateValidRequest();
        model.Title = string.Empty;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Validate_TitleTooLong_HasError()
    {
        var model = CreateValidRequest();
        model.Title = new string('A', 201);

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 200 characters.");
    }

    [Fact]
    public void Validate_ValidTitle_NoError()
    {
        var model = CreateValidRequest();
        model.Title = "Valid Event Title";

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    #endregion

    #region Description Tests

    [Fact]
    public void Validate_EmptyDescription_HasError()
    {
        var model = CreateValidRequest();
        model.Description = string.Empty;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description is required.");
    }

    [Fact]
    public void Validate_DescriptionTooLong_HasError()
    {
        var model = CreateValidRequest();
        model.Description = new string('A', 2001);

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 2000 characters.");
    }

    [Fact]
    public void Validate_ValidDescription_NoError()
    {
        var model = CreateValidRequest();
        model.Description = "A valid event description.";

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    #endregion

    #region Location Tests

    [Fact]
    public void Validate_EmptyLocation_HasError()
    {
        var model = CreateValidRequest();
        model.Location = string.Empty;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Location is required.");
    }

    [Fact]
    public void Validate_LocationTooLong_HasError()
    {
        var model = CreateValidRequest();
        model.Location = new string('A', 501);

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Location cannot exceed 500 characters.");
    }

    [Fact]
    public void Validate_ValidLocation_NoError()
    {
        var model = CreateValidRequest();
        model.Location = "123 Main Street";

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Location);
    }

    #endregion

    #region StartTime Tests

    [Fact]
    public void Validate_EmptyStartTime_HasError()
    {
        var model = CreateValidRequest();
        model.StartTime = default;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.StartTime)
            .WithErrorMessage("Start time is required.");
    }

    [Fact]
    public void Validate_ValidStartTime_NoError()
    {
        var model = CreateValidRequest();
        model.StartTime = DateTime.UtcNow.AddDays(14);

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.StartTime);
    }

    #endregion

    #region DurationMinutes Tests

    [Fact]
    public void Validate_ZeroDuration_HasError()
    {
        var model = CreateValidRequest();
        model.DurationMinutes = 0;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes)
            .WithErrorMessage("Duration must be at least 1 minute.");
    }

    [Fact]
    public void Validate_NegativeDuration_HasError()
    {
        var model = CreateValidRequest();
        model.DurationMinutes = -10;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes)
            .WithErrorMessage("Duration must be at least 1 minute.");
    }

    [Fact]
    public void Validate_DurationExceeds24Hours_HasError()
    {
        var model = CreateValidRequest();
        model.DurationMinutes = 1441;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes)
            .WithErrorMessage("Duration cannot exceed 24 hours (1440 minutes).");
    }

    [Fact]
    public void Validate_ValidDuration_NoError()
    {
        var model = CreateValidRequest();
        model.DurationMinutes = 120;

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void Validate_MaxValidDuration_NoError()
    {
        var model = CreateValidRequest();
        model.DurationMinutes = 1440;

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.DurationMinutes);
    }

    #endregion

    #region Capacity Tests

    [Fact]
    public void Validate_ZeroCapacity_HasError()
    {
        var model = CreateValidRequest();
        model.Capacity = 0;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Capacity)
            .WithErrorMessage("Capacity must be at least 1.");
    }

    [Fact]
    public void Validate_NegativeCapacity_HasError()
    {
        var model = CreateValidRequest();
        model.Capacity = -5;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Capacity)
            .WithErrorMessage("Capacity must be at least 1.");
    }

    [Fact]
    public void Validate_CapacityExceedsLimit_HasError()
    {
        var model = CreateValidRequest();
        model.Capacity = 10001;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Capacity)
            .WithErrorMessage("Capacity cannot exceed 10,000.");
    }

    [Fact]
    public void Validate_ValidCapacity_NoError()
    {
        var model = CreateValidRequest();
        model.Capacity = 50;

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Capacity);
    }

    [Fact]
    public void Validate_MaxValidCapacity_NoError()
    {
        var model = CreateValidRequest();
        model.Capacity = 10000;

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Capacity);
    }

    #endregion

    #region ImageUrl Tests

    [Fact]
    public void Validate_NullImageUrl_NoError()
    {
        var model = CreateValidRequest();
        model.ImageUrl = null;

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public void Validate_EmptyImageUrl_NoError()
    {
        var model = CreateValidRequest();
        model.ImageUrl = string.Empty;

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public void Validate_ImageUrlTooLong_HasError()
    {
        var model = CreateValidRequest();
        model.ImageUrl = "https://example.com/" + new string('a', 500);

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ImageUrl)
            .WithErrorMessage("Image URL cannot exceed 500 characters.");
    }

    [Fact]
    public void Validate_InvalidImageUrl_HasError()
    {
        var model = CreateValidRequest();
        model.ImageUrl = "not-a-valid-url";

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ImageUrl)
            .WithErrorMessage("Image URL must be a valid URL.");
    }

    [Fact]
    public void Validate_FtpImageUrl_HasError()
    {
        var model = CreateValidRequest();
        model.ImageUrl = "ftp://example.com/image.jpg";

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ImageUrl)
            .WithErrorMessage("Image URL must be a valid URL.");
    }

    [Fact]
    public void Validate_ValidHttpImageUrl_NoError()
    {
        var model = CreateValidRequest();
        model.ImageUrl = "http://example.com/image.jpg";

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public void Validate_ValidHttpsImageUrl_NoError()
    {
        var model = CreateValidRequest();
        model.ImageUrl = "https://example.com/image.jpg";

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }

    #endregion

    #region RegistrationDeadline Tests

    [Fact]
    public void Validate_NullRegistrationDeadline_NoError()
    {
        var model = CreateValidRequest();
        model.RegistrationDeadline = null;

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.RegistrationDeadline);
    }

    [Fact]
    public void Validate_RegistrationDeadlineAfterStartTime_HasError()
    {
        var model = CreateValidRequest();
        model.StartTime = DateTime.UtcNow.AddDays(7);
        model.RegistrationDeadline = DateTime.UtcNow.AddDays(8);

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.RegistrationDeadline)
            .WithErrorMessage("Registration deadline must be before the event start time.");
    }

    [Fact]
    public void Validate_RegistrationDeadlineEqualsStartTime_HasError()
    {
        var model = CreateValidRequest();
        var eventTime = DateTime.UtcNow.AddDays(7);
        model.StartTime = eventTime;
        model.RegistrationDeadline = eventTime;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.RegistrationDeadline)
            .WithErrorMessage("Registration deadline must be before the event start time.");
    }

    [Fact]
    public void Validate_RegistrationDeadlineBeforeStartTime_NoError()
    {
        var model = CreateValidRequest();
        model.StartTime = DateTime.UtcNow.AddDays(7);
        model.RegistrationDeadline = DateTime.UtcNow.AddDays(5);

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.RegistrationDeadline);
    }

    #endregion

    #region Status Tests

    [Fact]
    public void Validate_ValidStatus_NoError()
    {
        var model = CreateValidRequest();
        model.Status = EventStatus.Active;

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Theory]
    [InlineData(EventStatus.Active)]
    [InlineData(EventStatus.Cancelled)]
    public void Validate_AllValidStatuses_NoError(EventStatus status)
    {
        var model = CreateValidRequest();
        model.Status = status;

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_InvalidStatus_HasError()
    {
        var model = CreateValidRequest();
        model.Status = (EventStatus)999;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Invalid event status.");
    }

    #endregion

    #region RequiredSkillIds Tests

    [Fact]
    public void Validate_NullRequiredSkillIds_NoError()
    {
        var model = CreateValidRequest();
        model.RequiredSkillIds = null;

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.RequiredSkillIds);
    }

    [Fact]
    public void Validate_EmptyRequiredSkillIds_NoError()
    {
        var model = CreateValidRequest();
        model.RequiredSkillIds = new List<int>();

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.RequiredSkillIds);
    }

    [Fact]
    public void Validate_UniqueRequiredSkillIds_NoError()
    {
        var model = CreateValidRequest();
        model.RequiredSkillIds = new List<int> { 1, 2, 3 };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.RequiredSkillIds);
    }

    [Fact]
    public void Validate_DuplicateRequiredSkillIds_HasError()
    {
        var model = CreateValidRequest();
        model.RequiredSkillIds = new List<int> { 1, 2, 2 };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.RequiredSkillIds)
            .WithErrorMessage("Skill IDs must be unique.");
    }

    #endregion

    #region Complete Request Tests

    [Fact]
    public void Validate_ValidRequest_NoErrors()
    {
        var model = CreateValidRequest();

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidRequestWithAllOptionalFields_NoErrors()
    {
        var model = CreateValidRequest();
        model.ImageUrl = "https://example.com/event.jpg";
        model.RegistrationDeadline = model.StartTime.AddDays(-1);
        model.RequiredSkillIds = new List<int> { 1, 2, 3 };
        model.Status = EventStatus.Active;

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
