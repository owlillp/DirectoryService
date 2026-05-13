using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Queries.GetChildDepartments;

public record GetChildDepartmentsQuery(Guid ParentId, GetChildDepartmentsRequest Request) : IQuery;