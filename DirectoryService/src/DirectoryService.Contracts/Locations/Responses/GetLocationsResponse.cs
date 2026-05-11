namespace DirectoryService.Contracts.Locations.Responses;

public record GetLocationsResponse(IReadOnlyList<LocationDto> Locations, long TotalCount);