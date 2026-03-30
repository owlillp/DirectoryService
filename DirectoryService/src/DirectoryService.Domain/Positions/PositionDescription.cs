using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Positions;

public record PositionDescription
{
    public const short MAX_LENGTH = 100;

    public string Value { get; }

    private PositionDescription(string value) => Value = value;

    public static Result<PositionDescription, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation("position.description.validation.error", "value cannot be empty");
        }

        if (value.Length > MAX_LENGTH)
        {
            return Error.Validation("position.description.validation.error", $"value must be less {MAX_LENGTH} characters");
        }

        return new PositionDescription(value);
    }
}