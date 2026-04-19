using CSharpFunctionalExtensions;
using Shared.Constants;
using Shared.Failures;

namespace DirectoryService.Domain.Positions;

public record PositionName
{
    // EF Core
    private PositionName() { }

    private PositionName(string value) => Value = value;

    public string Value { get; } = string.Empty;

    public static Result<PositionName, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired(nameof(PositionName));
        }

        if (value.Length < LengthConstants.LENGTH_3 || value.Length > LengthConstants.LENGTH_100)
        {
            return GeneralErrors.InvalidLength(nameof(PositionName));
        }

        return new PositionName(value);
    }
}