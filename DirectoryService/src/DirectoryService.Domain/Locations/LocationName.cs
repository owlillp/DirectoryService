using CSharpFunctionalExtensions;
using Shared.Constants;
using Shared.Failures;

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
            return GeneralErrors.ValueIsRequired(nameof(LocationName));
        }

        if (value.Length < LengthConstants.LENGTH_3 || value.Length > LengthConstants.LENGTH_120)
        {
            return GeneralErrors.InvalidLength(nameof(LocationName));
        }

        return new LocationName(value);
    }
}