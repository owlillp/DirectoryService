using Core.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetTopDepartmentsByPositions;

public record GetTopDepartmentsByPositionQuery(int TopCount) : IQuery;