using FluentValidation;

public class SendSessionBookingRequestDtoValidator
    : AbstractValidator<CreateSessionBookingRequestDto>
{
    public SendSessionBookingRequestDtoValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage("Doctor ID is required.");

        RuleFor(x => x.PatientIsAvailableFromUtc)
            .Must(dt => dt.Kind == DateTimeKind.Utc)
            .WithMessage("'From' time must be in UTC format.")
            .Must(BeInTheFuture)
            .WithMessage("'From' time must be in the future.");

        RuleFor(x => x.PatientIsAvailableToUtc)
            .Must(dt => dt.Kind == DateTimeKind.Utc)
            .WithMessage("'To' time must be in UTC format.")
            .Must(BeInTheFuture)
            .WithMessage("'To' time must be in the future.")
            .Must((dto, to) => to > dto.PatientIsAvailableFromUtc)
            .WithMessage("'To' time must be after 'From' time.")
            .Must((dto, to) => (to - dto.PatientIsAvailableFromUtc) >= TimeSpan.FromHours(1))
            .WithMessage("The duration between 'From' and 'To' must be at least one hour.");

        RuleFor(x => x.PatientNote)
            .MaximumLength(1000)
            .WithMessage("Patient note must not exceed 1000 characters.");
    }

    private bool BeInTheFuture(DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }
}
