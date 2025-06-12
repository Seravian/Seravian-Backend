using FluentValidation;
using Seravian.DTOs.Doctor;

public class SendVerificationRequestDtoValidator
    : AbstractValidator<SendVerificationRequestRequestDto>
{
    private const long MaxTotalUploadSizeBytes = 15 * 1024 * 1024; // 15 MB
    private const int MaxFileCount = 10;
    private static readonly string[] AllowedContentTypes =
    {
        "image/png",
        "image/jpeg",
        "application/pdf",
    };

    public SendVerificationRequestDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Description)
            .NotNull()
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(2000)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Description cannot be whitespace.");

        RuleFor(x => x.SessionPrice)
            .GreaterThan(0)
            .WithMessage("Session price must be greater than 0.");

        RuleFor(x => x.Title).IsInEnum().WithMessage("Invalid doctor title is provided.");

        RuleFor(x => x.Attachments)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("At least one file is required.")
            .Must(attachments => attachments.Count > 0)
            .WithMessage("At least one file is required.")
            .Must(attachments => attachments.Count <= MaxFileCount)
            .WithMessage($"You can upload a maximum of {MaxFileCount} files.")
            .Must(attachments => !attachments.Any(attachment => attachment is null))
            .WithMessage("Any Attachment cannot be null.")
            .Must(attachments =>
                attachments.Sum(attachment => attachment.Length) <= MaxTotalUploadSizeBytes
            )
            .WithMessage($"Total file size must not exceed {MaxTotalUploadSizeBytes}MB.");

        RuleForEach(x => x.Attachments)
            .Cascade(CascadeMode.Stop)
            .ChildRules(file =>
            {
                file.RuleFor(f => f.FileName)
                    .NotEmpty()
                    .WithMessage("File name is required.")
                    .Must(IsValidFileName)
                    .WithMessage("Invalid file name.");

                file.RuleFor(f => f.ContentType)
                    .Must(type => AllowedContentTypes.Contains(type))
                    .WithMessage("Only PNG, JPG, and PDF files are allowed.");
            });
    }

    private bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            return false;

        var invalidChars = Path.GetInvalidFileNameChars();
        if (fileName.Any(c => invalidChars.Contains(c)))
            return false;

        if (fileName.Length > 100)
            return false;

        var reservedNames = new[] { "CON", "AUX", "NUL", "PRN", "COM1", "LPT1" };
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
        if (reservedNames.Contains(nameWithoutExtension))
            return false;

        return true;
    }
}
