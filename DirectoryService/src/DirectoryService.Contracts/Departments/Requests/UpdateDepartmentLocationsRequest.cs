namespace DirectoryService.Contracts.Departments.Requests;

public record UpdateDepartmentLocationsRequest(IEnumerable<Guid> LocationIds);