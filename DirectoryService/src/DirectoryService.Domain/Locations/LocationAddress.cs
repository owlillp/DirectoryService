using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Locations;

public record LocationAddress
{
    private LocationAddress(
        string country,
        string city,
        string street,
        int postalCode,
        int buildingNumber,
        string? apartment = null)
    {
        Country = country;
        City = city;
        Street = street;
        PostalCode = postalCode;
        BuildingNumber = buildingNumber;
        Apartment = apartment;
    }

    public string Country { get; }

    public string City { get; }

    public string Street { get; }

    public int PostalCode { get; }

    public int BuildingNumber { get; }

    public string? Apartment { get; }

    public static Result<LocationAddress, Error> Create(
        string country,
        string city,
        string street,
        int postalCode,
        int buildingNumber,
        string? apartment = null)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return Error.Validation("location.address.validation.error", "country cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return Error.Validation("location.city.validation.error", "city cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(street))
        {
            return Error.Validation("location.street.validation.error", "street cannot be empty");
        }

        if (postalCode < 0)
        {
            return Error.Validation("location.postalcode.validation.error", "postal code cannot be less zero");
        }

        if (buildingNumber < 0)
        {
            return Error.Validation("location.building.number.validation.error", "building number cannot be less zero");
        }

        return new LocationAddress(
            country,
            city,
            street,
            postalCode,
            buildingNumber,
            apartment);
    }
}