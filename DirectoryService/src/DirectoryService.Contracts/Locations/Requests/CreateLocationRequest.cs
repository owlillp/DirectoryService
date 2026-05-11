namespace DirectoryService.Contracts.Locations.Requests;

public record CreateLocationRequest(
    string Name,
    LocationAddressDto Address,
    string TimeZone);