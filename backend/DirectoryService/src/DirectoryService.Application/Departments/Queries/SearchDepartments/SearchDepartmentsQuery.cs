using Core.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Queries.SearchDepartments;

public record SearchDepartmentsQuery(SearchDepartmentsRequest Request) : IQuery;