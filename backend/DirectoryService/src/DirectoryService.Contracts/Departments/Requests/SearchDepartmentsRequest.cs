namespace DirectoryService.Contracts.Departments.Requests;

public record SearchDepartmentsRequest(
    string Search,
    bool? IsActive,
    Guid? ParentId,
    Guid[]? LocationIds,
    Guid[]? ExcludeIds,
    string SortBy = "name",
    string SortDirection = "asc",
    int Page = 1,
    int PageSize = 10);