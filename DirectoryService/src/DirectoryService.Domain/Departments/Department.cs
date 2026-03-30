using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public sealed class Department
{
    private Department() { }

    private List<DepartmentPosition> _positions = [];
    private List<DepartmentLocation> _locations = [];

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
        string name,
        string identifier,
        Guid? parentId,
        string path,
        short depth,
        Guid? id = null)
    {
        var nameResult = DepartmentName.Create(name);
        if (nameResult.IsFailure)
        {
            return nameResult.Error;
        }

        var identifierResult = DepartmentIdentifier.Create(identifier);
        if (identifierResult.IsFailure)
        {
            return identifierResult.Error;
        }

        var pathResult = DepartmentPath.Create(path);
        if (pathResult.IsFailure)
        {
            return pathResult.Error;
        }

        return new Department(
            id ?? Guid.NewGuid(),
            nameResult.Value,
            identifierResult.Value,
            parentId,
            pathResult.Value,
            depth);
    }
}