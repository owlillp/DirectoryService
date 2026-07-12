namespace DirectoryService.Contracts.Common;

public record CursorPaginationRequest(
    Guid? Cursor,
    int Limit = 20
);