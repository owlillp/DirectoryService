namespace DirectoryService.Contracts.Common;

public record PagedResult<T>(IReadOnlyList<T> Records, long TotalCount);