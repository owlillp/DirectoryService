using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Requests;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public record GetLocationsQuery(GetLocationsRequest Request) : IQuery;