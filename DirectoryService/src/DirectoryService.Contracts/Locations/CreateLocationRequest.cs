namespace DirectoryService.Contracts.Locations;

public record CreateLocationRequest
{
    public string Name { get; set; } = null!;
    public LocationAddressDto Address { get; set; } = null!;
    public string TimeZone { get; set; } = null!;
}