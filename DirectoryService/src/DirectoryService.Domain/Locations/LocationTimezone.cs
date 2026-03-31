using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using TimeZoneConverter;

namespace DirectoryService.Domain.Locations;

public record LocationTimezone
{
    private LocationTimezone(string value) => Value = value;

    public string Value { get; }

    public static Result<LocationTimezone, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation("location.timezone.validation.error", "value cannot be empty");
        }

        if (!TZConvert.KnownIanaTimeZoneNames.Contains(value))
        {
            return Error.Validation("location.timezone.validation.error", "value must be IANA timezone");
        }

        return new LocationTimezone(value);
    }
}