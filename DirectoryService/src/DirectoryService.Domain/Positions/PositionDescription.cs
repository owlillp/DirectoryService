using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared.Constants;
using DirectoryService.Domain.Shared.Errors;

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
            return Error.Validation("position.description.validation.error", "value cannot be empty");
        }

        if (value.Length > LengthConstants.LENGTH_100)
        {
            return Error.Validation("position.description.validation.error", $"value must be less {LengthConstants.LENGTH_100} characters");
        }

        return new PositionDescription(value);
    }
}