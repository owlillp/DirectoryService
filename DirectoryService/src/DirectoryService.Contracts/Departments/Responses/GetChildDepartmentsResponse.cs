using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Departments.Dtos;

namespace DirectoryService.Contracts.Departments.Responses;

public record GetChildDepartmentsResponse(Guid ParentId, PagedResult<DepartmentWithChildrenDto> PagedResult);