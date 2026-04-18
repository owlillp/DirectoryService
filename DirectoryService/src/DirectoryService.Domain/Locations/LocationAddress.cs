using CSharpFunctionalExtensions;
using Shared.Failures;

namespace DirectoryService.Domain.Locations;

public record LocationAddress
{
    // EF Core
    private LocationAddress() { }

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

    public string Country { get; } = string.Empty;

    public string City { get; } = string.Empty;

    public string Street { get; } = string.Empty;

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
            return GeneralErrors.FieldIsRequired(nameof(LocationAddress), nameof(Country));
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return GeneralErrors.FieldIsRequired(nameof(LocationAddress), nameof(City));
        }

        if (string.IsNullOrWhiteSpace(street))
        {
            return GeneralErrors.FieldIsRequired(nameof(LocationAddress), nameof(Street));
        }

        if (postalCode < 0)
        {
            return GeneralErrors.NegativeValue(nameof(LocationAddress), nameof(PostalCode));
        }

        if (buildingNumber < 0)
        {
            return GeneralErrors.NegativeValue(nameof(LocationAddress), nameof(buildingNumber));
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