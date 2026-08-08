using DirectoryService.Domain.DepartmentLocations;

namespace DirectoryService.Domain.Locations;

public sealed class Location
{
    private readonly List<DepartmentLocation> _departments = [];

    // EF Core
    private Location() { }

    private Location(
        LocationId id,
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

    public LocationId Id { get; private init; } = null!;

    public LocationName Name { get; private set; } = null!;

    public LocationAddress Address { get; private set; } = null!;

    public LocationTimezone Timezone { get; private set; } = null!;

    public Guid? PreviewId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public IReadOnlyList<DepartmentLocation> Departments => _departments;

    public static Location Create(
        LocationName name,
        LocationAddress address,
        LocationTimezone timezone,
        LocationId? id = null)
    {
        return new Location(
            id ?? new LocationId(Guid.NewGuid()),
            name,
            address,
            timezone);
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Rename(LocationName name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(LocationAddress address)
    {
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTimezone(LocationTimezone timezone)
    {
        Timezone = timezone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePreviewId(Guid? previewId)
    {
        PreviewId = previewId;
        UpdatedAt = DateTime.UtcNow;
    }
}