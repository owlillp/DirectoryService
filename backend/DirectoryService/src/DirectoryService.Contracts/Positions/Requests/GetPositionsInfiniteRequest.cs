using DirectoryService.Contracts.Common;

namespace DirectoryService.Contracts.Positions.Requests;

public record GetPositionsInfiniteRequest(
    InfinitePaginationRequest InfiniteRequest,
    string? Search,
    bool? IsActive,
    string? SortBy = "name",
    string? SortDirection = "asc"
);

