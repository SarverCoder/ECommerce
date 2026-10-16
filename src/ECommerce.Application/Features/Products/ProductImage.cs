using FluentValidation;

namespace ECommerce.Application.Features.Products;

/// <summary>Framework-agnostic upload (the API layer adapts IFormFile to this).</summary>
public record UploadImageRequest(Stream Content, string FileName, long Length);

public static class ProductImageRules
{
    public const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] Riff = "RIFF"u8.ToArray();
    private static readonly byte[] Webp = "WEBP"u8.ToArray();

    public static readonly IReadOnlySet<string> AllowedExtensions =
        new HashSet<string>([".jpg", ".jpeg", ".png", ".webp"], StringComparer.OrdinalIgnoreCase);

    public static string GetExtension(string fileName) => Path.GetExtension(fileName).ToLowerInvariant();

    /// <summary>Checks the file's magic bytes so a renamed non-image can't pass as one.</summary>
    public static bool HasValidSignature(Stream content, string extension)
    {
        if (!content.CanSeek)
            return false;

        Span<byte> header = stackalloc byte[12];
        content.Position = 0;
        var read = content.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);
        content.Position = 0;
        header = header[..read];

        return extension switch
        {
            ".jpg" or ".jpeg" => header.StartsWith(Jpeg),
            ".png" => header.StartsWith(Png),
            ".webp" => header.Length == 12 && header[..4].SequenceEqual(Riff) && header[8..12].SequenceEqual(Webp),
            _ => false
        };
    }
}

public class UploadImageRequestValidator : AbstractValidator<UploadImageRequest>
{
    public UploadImageRequestValidator()
    {
        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("An image file is required.")
            .LessThanOrEqualTo(ProductImageRules.MaxBytes).WithMessage("Image must be at most 5 MB.")
            .OverridePropertyName("file");

        RuleFor(x => x.FileName)
            .Must(name => ProductImageRules.AllowedExtensions.Contains(ProductImageRules.GetExtension(name)))
            .WithMessage("Allowed formats: .jpg, .jpeg, .png, .webp.")
            .When(x => x.Length > 0)
            .OverridePropertyName("file");

        RuleFor(x => x)
            .Must(x => ProductImageRules.HasValidSignature(x.Content, ProductImageRules.GetExtension(x.FileName)))
            .WithMessage("File content is not a valid image of the declared format.")
            .When(x => x.Length is > 0 and <= ProductImageRules.MaxBytes
                       && ProductImageRules.AllowedExtensions.Contains(ProductImageRules.GetExtension(x.FileName)))
            .OverridePropertyName("file");
    }
}
