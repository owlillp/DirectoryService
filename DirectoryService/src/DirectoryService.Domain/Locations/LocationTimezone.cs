using CSharpFunctionalExtensions;
using Shared.Failures;
using TimeZoneConverter;

namespace DirectoryService.Domain.Locations;

public record LocationTimezone
{
    private LocationTimezone() { }

    private LocationTimezone(string value) => Value = value;

    public string Value { get; } = string.Empty;

    public static Result<LocationTimezone, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired(nameof(LocationTimezone));
        }

        if (!TZConvert.KnownIanaTimeZoneNames.Contains(value))
        {
            return GeneralErrors.InvalidIANACode(nameof(LocationTimezone));
        }

        return new LocationTimezone(value);
    }
}