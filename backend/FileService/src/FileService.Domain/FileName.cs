using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace FileService.Domain;

public sealed record FileName
{
    public string Value { get; } = null!;
    public string Extension { get; } = null!;

    // EF Core
    private FileName() { }

    private FileName(string value, string extension)
    {
        Value = value;
        Extension = extension;
    }

    public static Result<FileName, Error> Create(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return GeneralErrors.ValueIsInvalid(nameof(fileName));

        int lastDot = fileName.LastIndexOf('.');
        if(lastDot == -1 || lastDot == fileName.Length - 1)
            return GeneralErrors.ValueIsInvalid("fileName", "File must have extension");

        string extension = fileName[(lastDot + 1)..].ToLowerInvariant();
        return new FileName(fileName, extension);
    }
}