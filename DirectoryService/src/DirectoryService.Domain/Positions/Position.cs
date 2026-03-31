using DirectoryService.Domain.DepartmentPositions;

namespace DirectoryService.Domain.Positions;

public class Position
{
    private Position() { }

    private List<DepartmentPosition> _departments = [];

    private Position(Guid id, PositionName name, PositionDescription? description)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private init; }

    public PositionName Name { get; private set; } = null!;

    public PositionDescription? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> Departments => _departments;

    public static Position Create(PositionName name, PositionDescription? description, Guid? id = null)
    {
        return new Position(id ?? Guid.NewGuid(), name, description);
    }
}