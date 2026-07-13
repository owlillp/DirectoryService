namespace DirectoryService.Contracts.Common;

public record InfinitePagedResult<T>(
    IReadOnlyList<T> Records,
    Cursor? NextCursor,
    bool HasNextPage
);

public record InfinitePaginationRequest(Cursor? Cursor, int Limit);

public record Cursor(Guid Id, string? Value);
