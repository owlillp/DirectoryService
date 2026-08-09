using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.Failures;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Locations.Commands.SoftDelete;

public class SoftDeleteLocationHandler(
    ILogger<SoftDeleteLocationHandler> logger,
    IValidator<SoftDeleteLocationCommand> validator,
    ITransactionManager transactionManager,
    ILocationsRepository locationsRepository,
    LocationCacheInvalidator cacheInvalidator)
    : ICommandHandler<SoftDeleteLocationCommand>
{
    public async Task<UnitResult<Errors>> Handle(SoftDeleteLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var locationId = new LocationId(command.LocationId);

        var getLocationResult = await locationsRepository.GetByIdWithLock(locationId, cancellationToken);
        if (getLocationResult.IsFailure)
        {
            return getLocationResult.Error.ToErrors();
        }

        var location = getLocationResult.Value;

        if (!location.IsActive)
        {
            return LocationsErrors.Inactive(locationId.Value).ToErrors();
        }

        location.Deactivate();

        var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error.ToErrors();
        }

        await cacheInvalidator.InvalidateLocationAsync(locationId.Value, cancellationToken);

        logger.LogInformation("Success soft delete location with id [{locationId}]", locationId.Value);

        return UnitResult.Success<Errors>();
    }
}