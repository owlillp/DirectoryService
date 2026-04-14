namespace DirectoryService.Domain.DepartmentPositions;

public class DepartmentPosition
{
    // EF Core
    private DepartmentPosition() { }

    public Guid Id { get; private init; }

    public Guid DepartmentId { get; private init; }

    public Guid PositionId { get; private init; }

    public DepartmentPosition(
        Guid id,
        Guid departmentId,
        Guid positionId)
    {
        Id = id;
        DepartmentId = departmentId;
        PositionId = positionId;
    }
}