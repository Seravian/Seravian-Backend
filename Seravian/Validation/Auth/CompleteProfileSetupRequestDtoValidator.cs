using FluentValidation;

public class CompleteProfileSetupRequestDtoValidator
    : AbstractValidator<CompleteProfileSetupRequestDto>
{
    public CompleteProfileSetupRequestDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .NotNull()
            .MaximumLength(100)
            .Matches(@"^[A-Za-z]+(?:[ '-][A-Za-z]+)*$")
            .WithMessage(
                "Full name should only contain letters, spaces, hyphens, and apostrophes."
            );

        RuleFor(x => x.Role)
            .NotNull()
            .Must(role => role == UserRole.Doctor || role == UserRole.Patient)
            .WithMessage("Role must be either Doctor or Patient.");

        // DateOfBirth validation rules
        RuleFor(x => x.DateOfBirth)
            .NotNull()
            .WithMessage("Date of birth is required.")
            .Must(x => BeNotInFuture(x))
            .WithMessage("Date of birth cannot be in the future.")
            .Must(x => BeReasonableAge(x))
            .WithMessage("Date of birth is too far in the past.");

        RuleFor(x => x.Gender).NotNull();
    }

    private bool BeAValidAge(DateOnly? dateOfBirth)
    {
        if (!dateOfBirth.HasValue)
            return false;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return today.Year - dateOfBirth.Value.Year >= 18;
    }

    // Custom rule to check if the date is not in the future
    private bool BeNotInFuture(DateOnly? dateOfBirth)
    {
        if (!dateOfBirth.HasValue)
            return true;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return dateOfBirth.Value <= today;
    }

    // Custom rule to check if the date is a reasonable age (not too old, e.g., older than 120 years)
    private bool BeReasonableAge(DateOnly? dateOfBirth)
    {
        if (!dateOfBirth.HasValue)
            return true;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var maxAge = today.AddYears(-120); // Maximum age (120 years)
        return dateOfBirth.Value >= maxAge;
    }
}
