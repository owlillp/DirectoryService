namespace DirectoryService.Contracts.Departments.Requests;

public record SearchDepartmentRequest(string Name, int Page = 1, int PageSize = 10);