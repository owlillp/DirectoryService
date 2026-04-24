namespace DirectoryService.Contracts.Locations;

public record CreateLocationRequest(
    string Name,
    LocationAddressDto Address,
    string TimeZone);