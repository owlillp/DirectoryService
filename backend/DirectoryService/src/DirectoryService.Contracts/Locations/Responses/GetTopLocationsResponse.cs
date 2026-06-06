using DirectoryService.Contracts.Locations.Dtos;

namespace DirectoryService.Contracts.Locations.Responses;

public record GetTopLocationsResponse(IReadOnlyList<TopLocationDto> Locations, int Count);