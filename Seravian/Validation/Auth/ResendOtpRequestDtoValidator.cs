using FluentValidation;

public class ResendOtpRequestDtoValidator : AbstractValidator<ResendOtpRequestDto>
{


    public ResendOtpRequestDtoValidator()
    {

        // Email validation
        RuleFor(x => x.Email)
            .NotNull()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.")
            .MaximumLength(100)
            .WithMessage("Email must not exceed 100 characters.");


    }
}
