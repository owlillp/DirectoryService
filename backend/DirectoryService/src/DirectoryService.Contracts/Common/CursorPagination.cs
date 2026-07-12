namespace DirectoryService.Contracts.Common;

public record CursorPagedResult<T>(
    IReadOnlyList<T> Records,
    Cursor? NextCursor,
    bool HasNextPage
);

public record CursorPaginationRequest(Cursor? Cursor, int Limit);

public record Cursor(Guid Id, string? Value);

