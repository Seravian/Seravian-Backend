using FluentValidation;

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
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(2000)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Description cannot be whitespace.");

        RuleFor(x => x.Title).IsInEnum().WithMessage("Invalid doctor title is provided.");

        RuleFor(x => x.Attachments)
            .NotNull()
            .WithMessage("At least one file is required.")
            .Must(files => files.Count > 0)
            .WithMessage("At least one file is required.")
            .Must(files => files.Count <= MaxFileCount)
            .WithMessage($"You can upload a maximum of {MaxFileCount} files.")
            .Must(files => files.Sum(f => f.Length) <= MaxTotalUploadSizeBytes)
            .WithMessage($"Total file size must not exceed {MaxTotalUploadSizeBytes}MB.");

        RuleForEach(x => x.Attachments)
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
