using Core.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentAncestors;

public record GetDepartmentAncestorsQuery(Guid TargetDepartmentId) : IQuery;