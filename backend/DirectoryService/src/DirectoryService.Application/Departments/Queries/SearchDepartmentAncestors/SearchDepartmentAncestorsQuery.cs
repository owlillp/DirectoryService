using Core.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Queries.SearchDepartmentAncestors;

public record SearchDepartmentAncestorsQuery(SearchDepartmentAncestorsRequest AncestorsRequest) : IQuery;