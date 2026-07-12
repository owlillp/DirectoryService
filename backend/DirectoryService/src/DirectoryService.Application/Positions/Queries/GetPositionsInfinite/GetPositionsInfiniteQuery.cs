using Core.Abstractions;
using DirectoryService.Contracts.Positions.Requests;

namespace DirectoryService.Application.Positions.Queries.GetPositionsInfinite;

public record GetPositionsInfiniteQuery(GetPositionsInfiniteRequest InfiniteRequest) : IQuery;