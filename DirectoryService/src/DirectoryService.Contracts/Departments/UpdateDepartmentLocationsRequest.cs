namespace DirectoryService.Contracts.Departments;

public record UpdateDepartmentLocationsRequest(IEnumerable<Guid> LocationIds);