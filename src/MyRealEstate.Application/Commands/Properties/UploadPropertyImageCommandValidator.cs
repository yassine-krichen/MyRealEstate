using FluentValidation;

namespace MyRealEstate.Application.Commands.Properties;

public class UploadPropertyImageCommandValidator : AbstractValidator<UploadPropertyImageCommand>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    
    public UploadPropertyImageCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property ID is required");
        
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .Must(HaveValidExtension).WithMessage($"Only image files are allowed: {string.Join(", ", AllowedExtensions)}");
        
        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File stream is required")
            .Must(stream => stream != Stream.Null && stream.Length > 0).WithMessage("File cannot be empty")
            .Must(stream => stream.Length <= MaxFileSize).WithMessage($"File size must not exceed {MaxFileSize / 1024 / 1024}MB");
    }
    
    private bool HaveValidExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }
}
