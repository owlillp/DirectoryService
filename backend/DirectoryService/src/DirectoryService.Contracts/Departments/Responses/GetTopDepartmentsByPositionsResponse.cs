using DirectoryService.Contracts.Departments.Dtos;

namespace DirectoryService.Contracts.Departments.Responses;

public record GetTopDepartmentsByPositionsResponse(IReadOnlyList<TopDepartmentByPositionsDto> Departments, int Count);