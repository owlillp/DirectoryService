using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Locations.Commands.UpdateLocation;

public class UpdateLocationHandler (
    ILogger<UpdateLocationHandler> logger,
    IValidator<UpdateLocationCommand> validator,
    ILocationsRepository repository,
    ITransactionManager transactionManager
    ) : ICommandHandler<UpdateLocationCommand>
{
    public async Task<UnitResult<Errors>> Handle(UpdateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var locationId = new LocationId(command.LocationId);
        var getLocationResult = await repository.GetByAsync(l => l.Id == locationId && l.IsActive, cancellationToken);
        if (getLocationResult.IsFailure)
        {
            return getLocationResult.Error.ToErrors();
        }

        var location = getLocationResult.Value;
        var request = command.Request;

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = LocationName.Create(request.Name).Value;
            location.Rename(name);
        }

        if (request.Address != null)
        {
            var address = LocationAddress.Create(
                request.Address.Country,
                request.Address.City,
                request.Address.Street,
                request.Address.PostalCode,
                request.Address.BuildingNumber,
                request.Address.Apartment).Value;
            location.UpdateAddress(address);
        }

        if (request.TimeZone != null)
        {
            var timezone = LocationTimezone.Create(request.TimeZone).Value;
            location.UpdateTimezone(timezone);
        }

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }

        logger.LogInformation(
            "Success update location with id [{locationId}]",
            location.Id.Value);

        return UnitResult.Success<Errors>();
    }
}