using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public sealed class Department
{
    private Department() { }

    private readonly List<DepartmentPosition> _positions = [];
    private readonly List<DepartmentLocation> _locations = [];

    private Department(
        Guid id,
        DepartmentName name,
        DepartmentIdentifier identifier,
        Guid? parentId,
        DepartmentPath path,
        short depth)
    {
        Id = id;
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private init; }

    public DepartmentName Name { get; private set; } = null!;

    public DepartmentIdentifier Identifier { get; private set; } = null!;

    public Guid? ParentId { get; private set; }

    public DepartmentPath Path { get; private set; } = null!;

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> Positions => _positions;

    public IReadOnlyList<DepartmentLocation> Locations => _locations;

    public static Result<Department, Error> Create(
        DepartmentName name,
        DepartmentIdentifier identifier,
        DepartmentPath path,
        Guid? parentId,
        short depth,
        Guid? id = null)
    {
        if (depth < 0)
        {
            return Error.Validation("department.depth.validation.error", "depth cannot be less zero");
        }

        return new Department(
            id ?? Guid.NewGuid(),
            name,
            identifier,
            parentId,
            path,
            depth);
    }
}