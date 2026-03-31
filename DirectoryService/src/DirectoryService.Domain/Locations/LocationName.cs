using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Locations;

public record LocationName
{
    public const short MIN_LENGTH = 3;
    public const short MAX_LENGTH = 120;

    public string Value { get; }

    private LocationName(string value) => Value = value;

    public static Result<LocationName, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation("location.name.validation.error", "value cannot be empty");
        }

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return Error.Validation("location.name.validation.error", $"value must be between {MIN_LENGTH} and {MAX_LENGTH} characters long");
        }

        return new LocationName(value);
    }
}