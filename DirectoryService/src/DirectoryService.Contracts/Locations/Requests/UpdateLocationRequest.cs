using DirectoryService.Contracts.Locations.Dtos;

namespace DirectoryService.Contracts.Locations.Requests;

public record UpdateLocationRequest
{
    public string? Name { get; init; }
    public LocationAddressDto? Address { get; init; }
    public string? TimeZone { get; init; }
}