namespace DirectoryService.Contracts.Common;

public record CursorPagedResult<T>(
    IReadOnlyList<T> Records,
    Guid? NextCursor,
    bool HasNextPage
);