using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared.Constants;
using DirectoryService.Domain.Shared.Errors;

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
            return Error.Validation("position.name.validation.error", "value cannot be empty");
        }

        if (value.Length < LengthConstants.LENGTH_3 || value.Length > LengthConstants.LENGTH_100)
        {
            return Error.Validation("position.name.validation.error", $"value must be between {LengthConstants.LENGTH_3} and {LengthConstants.LENGTH_100} characters long");
        }

        return new PositionName(value);
    }
}