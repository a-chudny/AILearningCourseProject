using FluentValidation;
using VolunteerPortal.API.Models.DTOs.Events;

namespace VolunteerPortal.API.Validators;

/// <summary>
/// Validator for UpdateEventRequest.
/// </summary>
public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
{
    public UpdateEventRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(500).WithMessage("Location cannot exceed 500 characters.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be at least 1 minute.")
            .LessThanOrEqualTo(1440).WithMessage("Duration cannot exceed 24 hours (1440 minutes).");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be at least 1.")
            .LessThanOrEqualTo(10000).WithMessage("Capacity cannot exceed 10,000.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters.")
            .Must(BeValidUrlOrNull).WithMessage("Image URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.RegistrationDeadline)
            .Must((request, deadline) => BeBeforeStartTime(deadline, request.StartTime))
            .WithMessage("Registration deadline must be before the event start time.")
            .When(x => x.RegistrationDeadline.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid event status.");

        RuleFor(x => x.RequiredSkillIds)
            .Must(BeUniqueSkillIds).WithMessage("Skill IDs must be unique.")
            .When(x => x.RequiredSkillIds != null && x.RequiredSkillIds.Any());
    }

    private bool BeBeforeStartTime(DateTime? deadline, DateTime startTime)
    {
        if (!deadline.HasValue)
            return true;

        return deadline.Value < startTime;
    }

    private bool BeValidUrlOrNull(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private bool BeUniqueSkillIds(List<int> skillIds)
    {
        return skillIds.Distinct().Count() == skillIds.Count;
    }
}
