using DirectoryService.Contracts.Common;

namespace DirectoryService.Contracts.Positions.Requests;

public record GetCursorPositionsRequest(
    CursorPaginationRequest CursorRequest,
    string? Search,
    bool? IsActive,
    string? SortBy = "name",
    string? SortDirection = "asc"
);

