using FluentValidation;
using Seravian.DTOs.DoctorSessions;

public class AcceptPendingSessionBookingRequestDtoValidator
    : AbstractValidator<AcceptPendingSessionBookingRequestDto>
{
    public AcceptPendingSessionBookingRequestDtoValidator()
    {
        RuleFor(x => x.SessionBookingId).NotEmpty().WithMessage("SessionBookingId is required.");

        RuleFor(x => x.ScheduledAtUtc)
            .NotEmpty()
            .WithMessage("ScheduledAtUtc is required.")
            .Must(dt => dt.Kind == DateTimeKind.Utc)
            .WithMessage("ScheduledAtUtc time must be in UTC format.")
            .Must(scheduled => scheduled > DateTime.UtcNow.AddHours(1))
            .WithMessage("Scheduled time must be at least one hour from now.");
    }
}
