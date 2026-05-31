using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetDepartment;

public record GetDepartmentQuery(Guid DepartmentId) : IQuery;