using FluentValidation;
using Microsoft.Extensions.Options;

public class VerifyOtpRequestDtoValidator : AbstractValidator<VerifyOtpRequestDto>
{
    int _otpLength;

    public VerifyOtpRequestDtoValidator(IOptions<OtpSettings> otpSettings)
    {
        _otpLength = otpSettings.Value.OtpLength;

        // Validate UserId (GUID)
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty()
            .WithMessage("UserId is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("UserId cannot be empty.");

        // OTP validation with dynamic length from config
        RuleFor(x => x.OtpCode)
            .NotNull()
            .WithMessage("OTP is required.")
            .Matches("^[a-zA-Z0-9]*$")
            .WithMessage("OTP must be alphanumeric.")
            .Length(_otpLength)
            .WithMessage($"OTP must be {_otpLength} characters.");
    }
}
