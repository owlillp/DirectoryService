using DirectoryService.Contracts.Departments.Dtos;

namespace DirectoryService.Contracts.Departments.Responses;

public record GetRootDepartmentsResponse(IReadOnlyList<DepartmentDto> Departments, long Count);