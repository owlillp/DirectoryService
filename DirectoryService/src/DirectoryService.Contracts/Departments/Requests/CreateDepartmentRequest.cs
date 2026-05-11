namespace DirectoryService.Contracts.Departments.Requests;

public record CreateDepartmentRequest(
    string Name,
    string Identifier,
    Guid? ParentId,
    IEnumerable<Guid> LocationIds);