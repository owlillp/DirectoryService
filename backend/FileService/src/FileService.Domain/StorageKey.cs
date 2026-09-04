using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace FileService.Domain;

public sealed record StorageKey
{
    public string Key { get; } = null!;
    public string? Prefix { get; }
    public string Location { get; } = null!;
    public string Value { get; } = null!;
    public string FullPath { get; } = null!;

    // EF Core
    private StorageKey() { }

    [JsonConstructor]
    private StorageKey(string location, string? prefix, string key)
    {
        Location = location;
        Prefix = prefix;
        Key = key;
        Value = string.IsNullOrWhiteSpace(prefix) ? Key : $"{Prefix}/{Key}";
        FullPath = $"{Location}/{Value}";
    }

    public static Result<StorageKey, Error> Create(string location, string? prefix, string key)
    {
        if (string.IsNullOrWhiteSpace(location))
            return GeneralErrors.ValueIsInvalid(nameof(location));

        var normalizedKeyResult = NormalizeSegment(key);
        if (normalizedKeyResult.IsFailure)
            return normalizedKeyResult.Error;

        var normalizedPrefixResult = NormalizePrefix(prefix);
        if (normalizedPrefixResult.IsFailure)
            return normalizedPrefixResult.Error;

        return new StorageKey(location.Trim(), normalizedPrefixResult.Value, normalizedKeyResult.Value);
    }

    private static Result<string, Error> NormalizeSegment(string? segment)
    {
        if(string.IsNullOrWhiteSpace(segment))
            return GeneralErrors.ValueIsInvalid(nameof(segment));

        string trimmed = segment.Trim();

        if(trimmed.Contains('/', StringComparison.Ordinal) || trimmed.Contains('\\', StringComparison.Ordinal))
            return GeneralErrors.ValueIsInvalid("key");

        return trimmed;
    }

    private static Result<string, Error> NormalizePrefix(string? prefix)
    {
        if(string.IsNullOrWhiteSpace(prefix))
            return string.Empty;

        string[] parts = prefix.Trim().Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<string> normalizedParts = [];
        foreach (string part in parts)
        {
            var normalizedPart = NormalizeSegment(part);
            if (normalizedPart.IsFailure)
                return normalizedPart;

            if(!string.IsNullOrWhiteSpace(normalizedPart.Value))
                normalizedParts.Add(normalizedPart.Value);
        }

        return string.Join("/", normalizedParts);
    }

    public Result<StorageKey, Error> AppendKey(string childKey)
    {
        if (string.IsNullOrWhiteSpace(childKey))
        {
            return GeneralErrors.ValueIsInvalid("childKey");
        }

        string prefix = Value;
        return Create(Location, prefix, childKey);
    }
}