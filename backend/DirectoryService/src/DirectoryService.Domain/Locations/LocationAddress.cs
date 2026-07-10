using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

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
            return GeneralErrors.FieldIsRequired("address", "country");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return GeneralErrors.FieldIsRequired("address", "city");
        }

        if (string.IsNullOrWhiteSpace(street))
        {
            return GeneralErrors.FieldIsRequired("address", "street");
        }

        if (postalCode < 0)
        {
            return GeneralErrors.NegativeValue("address", "postalCode");
        }

        if (buildingNumber < 0)
        {
            return GeneralErrors.NegativeValue("address", "buildingNumber");
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