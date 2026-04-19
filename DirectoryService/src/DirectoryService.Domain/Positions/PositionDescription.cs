using CSharpFunctionalExtensions;
using Shared.Constants;
using Shared.Failures;

namespace DirectoryService.Domain.Positions;

public record PositionDescription
{
    // EF Core
    private PositionDescription() { }

    private PositionDescription(string value) => Value = value;

    public string Value { get; } = string.Empty;

    public static Result<PositionDescription, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired(nameof(PositionDescription));
        }

        if (value.Length > LengthConstants.LENGTH_100)
        {
            return GeneralErrors.InvalidLength(nameof(PositionDescription));
        }

        return new PositionDescription(value);
    }
}