using DirectoryService.Contracts.Departments.Dtos;

namespace DirectoryService.Contracts.Departments.Responses;

public record GetChildDepartmentsResponse(Guid ParentId, IReadOnlyList<DepartmentDto> Departments, long Count);