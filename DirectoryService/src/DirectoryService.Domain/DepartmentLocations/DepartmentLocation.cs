using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Domain.DepartmentLocations;

public class DepartmentLocation
{
    // EF Core
    private DepartmentLocation() { }

    public DepartmentLocationId Id { get; private init; } = null!;

    public DepartmentId DepartmentId { get; private init; } = null!;

    public LocationId LocationId { get; private init; } = null!;

    public DepartmentLocation(
        DepartmentId departmentId,
        LocationId locationId,
        DepartmentLocationId? id = null)
    {
        Id = id ?? new DepartmentLocationId(Guid.NewGuid());
        DepartmentId = departmentId;
        LocationId = locationId;
    }
}