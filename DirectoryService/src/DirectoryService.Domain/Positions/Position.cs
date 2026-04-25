using DirectoryService.Domain.DepartmentPositions;

namespace DirectoryService.Domain.Positions;

public class Position
{
    private List<DepartmentPosition> _departments = [];

    // EF Core
    private Position() { }

    private Position(
        PositionId id,
        PositionName name,
        PositionDescription? description,
        IEnumerable<DepartmentPosition> departments)
    {
        _departments = departments.ToList();

        Id = id;
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public PositionId Id { get; private init; } = null!;

    public PositionName Name { get; private set; } = null!;

    public PositionDescription? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> Departments => _departments;

    public static Position Create(
        PositionName name,
        PositionDescription? description,
        IEnumerable<DepartmentPosition> departments,
        PositionId? id = null)
        => new(id ?? new PositionId(Guid.NewGuid()),
            name,
            description,
            departments);
}