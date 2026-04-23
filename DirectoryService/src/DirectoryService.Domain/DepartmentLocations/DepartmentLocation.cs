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
        DepartmentLocationId id,
        DepartmentId departmentId,
        LocationId locationId)
    {
        Id = id;
        DepartmentId = departmentId;
        LocationId = locationId;
    }
}