using Shared.Failures;

namespace DirectoryService.Application.Locations.Failures;

public static class LocationsErrors
{
    public static Error Inactive(Guid locationId)
        => Error.Validation("location.inactive", $"Location with id [{locationId}] is inactive");
}