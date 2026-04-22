namespace DirectoryService.Contracts.Locations;

public record LocationAddressDto(string Country, string City, string Street, string? Apartment, int PostalCode, int BuildingNumber);