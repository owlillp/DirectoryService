using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Domain.DepartmentPositions;

public class DepartmentPosition
{
    // EF Core
    private DepartmentPosition() { }

    public DepartmentPositionId Id { get; private init; }

    public DepartmentId DepartmentId { get; private init; }

    public PositionId PositionId { get; private init; }

    public DepartmentPosition(
        DepartmentPositionId id,
        DepartmentId departmentId,
        PositionId positionId)
    {
        Id = id;
        DepartmentId = departmentId;
        PositionId = positionId;
    }
}