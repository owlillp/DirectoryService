using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Locations.Commands.UpdateLocation;

public class UpdateLocationValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationValidator()
    {
        RuleFor(c => c.LocationId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdateLocationCommand.LocationId)));

        RuleFor(c => c.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdateLocationCommand.Request)));

        When(c => c.Request != null!, () =>
        {
            When(c => c.Request.Name != null, () =>
            {
                RuleFor(c => c.Request.Name)
                    .MustBeValueObject(LocationName.Create!);
            });

            When(c => c.Request.TimeZone != null, () =>
            {
                RuleFor(c => c.Request.TimeZone)
                    .MustBeValueObject(LocationTimezone.Create!);
            });

            When(c => c.Request.Address != null, () =>
            {
                RuleFor(c => c.Request.Address!)
                    .MustBeValueObject(address =>
                        LocationAddress.Create(
                            address.Country,
                            address.City,
                            address.Street,
                            address.PostalCode,
                            address.BuildingNumber,
                            address.Apartment));
            });
        });
    }
}