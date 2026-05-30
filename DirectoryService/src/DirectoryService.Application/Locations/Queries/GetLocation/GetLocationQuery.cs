using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.Queries.GetLocation;

public record GetLocationQuery(Guid LocationId) : IQuery;