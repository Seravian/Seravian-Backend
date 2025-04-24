using FluentValidation;
using Microsoft.Extensions.Options;

public class VerifyOtpRequestDtoValidator : AbstractValidator<VerifyOtpRequestDto>
{
    int _otpLength;

    public VerifyOtpRequestDtoValidator(IOptions<OtpSettings> otpSettings)
    {
        _otpLength = otpSettings.Value.OtpLength;

        // Email validation
        RuleFor(x => x.Email)
            .NotNull()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.")
            .MaximumLength(100)
            .WithMessage("Email must not exceed 100 characters.");

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
