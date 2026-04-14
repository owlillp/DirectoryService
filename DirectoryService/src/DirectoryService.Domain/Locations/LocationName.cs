using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared.Constants;
using DirectoryService.Domain.Shared.Errors;

namespace DirectoryService.Domain.Locations;

public record LocationName
{
    // EF Core
    private LocationName() { }

    private LocationName(string value) => Value = value;

    public string Value { get; } = string.Empty;

    public static Result<LocationName, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation("location.name.validation.error", "value cannot be empty");
        }

        if (value.Length < LengthConstants.LENGTH_3 || value.Length > LengthConstants.LENGTH_120)
        {
            return Error.Validation("location.name.validation.error", $"value must be between {LengthConstants.LENGTH_3} and {LengthConstants.LENGTH_120} characters long");
        }

        return new LocationName(value);
    }
}