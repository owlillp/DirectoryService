using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Queries.GetRootDepartments;

public record GetRootDepartmentsQuery(GetRootDepartmentsRequest Request) : IQuery;