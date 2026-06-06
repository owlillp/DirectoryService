namespace DirectoryService.Contracts.Locations.Dtos;

public record LocationAddressDto
{
    public string Country { get; init; } = null!;
    public string City { get; init; } = null!;
    public string Street { get; init; } = null!;
    public string? Apartment { get; init; }
    public int PostalCode { get; init; }
    public int BuildingNumber { get; init; }
}