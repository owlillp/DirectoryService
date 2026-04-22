using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
        RuleFor(c => c.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(CreateLocationCommand.Request)));

        RuleFor(c => c.Request.Name).MustBeValueObject(LocationName.Create);

        RuleFor(c => c.Request.TimeZone).MustBeValueObject(LocationTimezone.Create);

        RuleFor(c => c.Request.Address)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(CreateLocationRequest.Address)))
            .MustBeValueObject(address =>
                LocationAddress.Create(
                    address.Country,
                    address.City,
                    address.Street,
                    address.PostalCode,
                    address.BuildingNumber,
                    address.Apartment));
    }
}