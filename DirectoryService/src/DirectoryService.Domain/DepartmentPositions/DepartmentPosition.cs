using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Domain.DepartmentPositions;

public class DepartmentPosition
{
    // EF Core
    private DepartmentPosition() { }

    public DepartmentPositionId Id { get; private init; } = null!;

    public DepartmentId DepartmentId { get; private init; } = null!;

    public PositionId PositionId { get; private init; } = null!;

    public DepartmentPosition(
        DepartmentId departmentId,
        PositionId positionId,
        DepartmentPositionId? id = null)
    {
        Id = id ?? new DepartmentPositionId(Guid.NewGuid());
        DepartmentId = departmentId;
        PositionId = positionId;
    }
}