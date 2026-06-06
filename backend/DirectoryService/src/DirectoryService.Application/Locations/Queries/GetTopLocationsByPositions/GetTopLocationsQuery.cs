using Core.Abstractions;

namespace DirectoryService.Application.Locations.Queries.GetTopLocationsByPositions;

public record GetTopLocationsQuery(int TopCount) : IQuery;