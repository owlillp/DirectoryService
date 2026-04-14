using DirectoryService.Domain.DepartmentLocations;

namespace DirectoryService.Domain.Locations;

public sealed class Location
{
    private readonly List<DepartmentLocation> _departments = [];

    // EF Core
    private Location() { }

    private Location(
        Guid id,
        LocationName name,
        LocationAddress address,
        LocationTimezone timezone)
    {
        Id = id;
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private init; }

    public LocationName Name { get; private set; } = null!;

    public LocationAddress Address { get; private set; } = null!;

    public LocationTimezone Timezone { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentLocation> Departments => _departments;

    public static Location Create(
        LocationName name,
        LocationAddress address,
        LocationTimezone timezone,
        Guid? id = null)
    {
        return new Location(
            id ?? Guid.NewGuid(),
            name,
            address,
            timezone);
    }
}