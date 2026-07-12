namespace DirectoryService.Contracts.Common;

public record PagedResult<T>(IReadOnlyList<T> Records, long TotalCount);

public record PaginationRequest(int Page = 1, int PageSize = 10);