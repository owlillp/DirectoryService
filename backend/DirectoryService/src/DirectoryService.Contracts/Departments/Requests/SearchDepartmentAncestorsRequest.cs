namespace DirectoryService.Contracts.Departments.Requests;

public record SearchDepartmentAncestorsRequest(string Name, int Page = 1, int PageSize = 10);