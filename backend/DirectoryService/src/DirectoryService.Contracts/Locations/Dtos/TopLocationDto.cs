namespace DirectoryService.Contracts.Locations.Dtos;

public record TopLocationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public LocationAddressDto Address { get; init; } = null!;
    public int DepartmentsCount { get; init; }
}