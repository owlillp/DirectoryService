namespace DirectoryService.Contracts.Common;

public record PagedResult<T>(
    IReadOnlyList<T> Records,
    long TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public record PaginationRequest(int Page = 1, int PageSize = 10);