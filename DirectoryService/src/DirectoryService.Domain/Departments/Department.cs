using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using Shared.Failures;

namespace DirectoryService.Domain.Departments;

public sealed class Department
{
    private readonly List<DepartmentPosition> _positions = [];
    private readonly List<DepartmentLocation> _locations = [];

    // EF Core
    private Department() { }

    private Department(
        DepartmentId id,
        DepartmentName name,
        DepartmentIdentifier identifier,
        DepartmentPath path,
        DepartmentId? parentId,
        short depth,
        List<DepartmentLocation> locations)
    {
        _locations = locations;

        Id = id;
        Name = name;
        Identifier = identifier;
        Path = path;
        Depth = depth;
        ParentId = parentId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public DepartmentId Id { get; private init; } = null!;

    public DepartmentName Name { get; private set; } = null!;

    public DepartmentIdentifier Identifier { get; private set; } = null!;

    public DepartmentId? ParentId { get; private set; }

    public DepartmentPath Path { get; private set; } = null!;

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> Positions => _positions;

    public IReadOnlyList<DepartmentLocation> Locations => _locations;

    public static Result<Department, Error> CreateParent(
        DepartmentName name,
        DepartmentIdentifier identifier,
        IEnumerable<DepartmentLocation> locations,
        DepartmentId? id = null)
    {
        var departmentLocationsList = locations.ToList();

        if (departmentLocationsList.Count == 0)
        {
            return GeneralErrors.InvalidLength(nameof(Department), nameof(Locations));
        }

        var path = DepartmentPath.CreateParent(identifier);

        return new Department(
            id ?? new DepartmentId(Guid.NewGuid()),
            name,
            identifier,
            path,
            null,
            0,
            departmentLocationsList);
    }

    public static Result<Department, Error> CreateChild(
        DepartmentName name,
        DepartmentIdentifier identifier,
        Department parent,
        IEnumerable<DepartmentLocation> locations,
        DepartmentId? id = null)
    {
        var departmentLocationsList = locations.ToList();

        if (departmentLocationsList.Count == 0)
        {
            return GeneralErrors.InvalidLength(nameof(Department), nameof(Locations));
        }

        var path = parent.Path.CreateChild(identifier);
        short depth = (short)(parent.Depth + 1);

        return new Department(
            id ?? new DepartmentId(Guid.NewGuid()),
            name,
            identifier,
            path,
            parent.Id,
            depth,
            departmentLocationsList);
    }
}