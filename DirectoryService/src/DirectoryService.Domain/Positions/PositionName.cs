using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Positions;

public record PositionName
{
    public const short MIN_LENGTH = 3;
    public const short MAX_LENGTH = 100;

    public string Value { get; }

    private PositionName(string value) => Value = value;

    public static Result<PositionName, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation("position.name.validation.error", "value cannot be empty");
        }

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return Error.Validation("position.name.validation.error", $"value must be between {MIN_LENGTH} and {MAX_LENGTH} characters long");
        }

        return new PositionName(value);
    }
}