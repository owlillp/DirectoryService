using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Common;

namespace DirectoryService.Application.Departments.Queries.GetChildDepartments;

public record GetChildDepartmentsQuery(Guid ParentId, PaginationRequest Request) : IQuery;