namespace DirectoryService.Domain.DepartmentLocations;

public class DepartmentLocation
{
    // EF Core
    private DepartmentLocation() { }

    public Guid Id { get; private init; }

    public Guid DepartmentId { get; private init; }

    public Guid LocationId { get; private init; }

    public DepartmentLocation(
        Guid id,
        Guid departmentId,
        Guid locationId)
    {
        Id = id;
        DepartmentId = departmentId;
        LocationId = locationId;
    }
}