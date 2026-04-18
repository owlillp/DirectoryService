namespace DirectoryService.Contracts.Locations;

public record CreateLocationDto
{
    public string Name { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string? Apartment { get; set; } = null;
    public int PostalCode { get; set; }
    public int BuildingNumber { get; set; }
    public string TimeZone { get; set; } = null!;
}