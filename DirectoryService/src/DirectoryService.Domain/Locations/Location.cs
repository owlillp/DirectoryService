using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Locations;

public sealed class Location
{
    private Location() { }

    private List<DepartmentLocation> _departments;

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

    public static Result<Location, Error> Create(
        string name,
        string country,
        string city,
        string street,
        string buildingNumber,
        string? apartment,
        string postalCode,
        string timezone,
        Guid? id = null
    )
    {
        var nameResult = LocationName.Create(name);
        if (nameResult.IsFailure)
        {
            return nameResult.Error;
        }

        var addressResult = LocationAddress.Create(
            country,
            city,
            street,
            postalCode,
            buildingNumber,
            apartment);
        if (addressResult.IsFailure)
        {
            return addressResult.Error;
        }

        var timezoneResult = LocationTimezone.Create(timezone);
        if (timezoneResult.IsFailure)
        {
            return timezoneResult.Error;
        }

        return new Location(
            id ?? Guid.NewGuid(),
            nameResult.Value, 
            addressResult.Value, 
            timezoneResult.Value);
    }
}