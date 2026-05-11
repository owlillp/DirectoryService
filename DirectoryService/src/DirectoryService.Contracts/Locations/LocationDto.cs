namespace DirectoryService.Contracts.Locations;

public record LocationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string TimeZone { get; init; } = null!;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<Guid> DepartmentIds { get; init; } = [];
    public LocationAddressDto Address { get; set; } = null!;
}