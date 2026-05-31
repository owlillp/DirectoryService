using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Positions.Queries.GetPosition;

public record GetPositionQuery(Guid PositionId) : IQuery;