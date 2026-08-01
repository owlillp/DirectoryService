using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace FileService.Domain;

public sealed record ContentType
{
    public string Value { get; } = null!;
    public MediaType Category { get; }

    // EF Core
    private ContentType() { }

    private ContentType(string value, MediaType type)
    {
        Value = value;
        Category = type;
    }

    public static Result<ContentType, Error> Create(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return GeneralErrors.ValueIsInvalid(nameof(contentType));

        MediaType category = contentType switch
        {
            _ when contentType.Contains("video", StringComparison.InvariantCultureIgnoreCase) => MediaType.VIDEO,
            _ when contentType.Contains("audio", StringComparison.InvariantCultureIgnoreCase) => MediaType.AUDIO,
            _ when contentType.Contains("image", StringComparison.InvariantCultureIgnoreCase) => MediaType.IMAGE,
            _ when contentType.Contains("document", StringComparison.InvariantCultureIgnoreCase) => MediaType.DOCUMENT,
            _ => MediaType.UNKNOWN
        };

        return new ContentType(contentType, category);
    }
}