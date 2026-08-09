using Core.Abstractions;
using Core.Abstractions.Database;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using FileService.Contracts.Communication;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Locations.Commands.UpdateLocationPreview;

public class UpdateLocationPreviewHandler(
    ILogger<UpdateLocationPreviewHandler> logger,
    IFileCommunicationService fileService,
    ITransactionManager transactionManager,
    ILocationsRepository repository,
    LocationCacheInvalidator cacheInvalidator) : ICommandHandler<UpdateLocationPreviewCommand>
{
    private const string IMAGE_MEDIA_TYPE = "IMAGE";

    public async Task<UnitResult<Errors>> Handle(UpdateLocationPreviewCommand command, CancellationToken cancellationToken)
    {
        var locationId = new LocationId(command.LocationId);
        var getLocationResult = await repository.GetByAsync(l => l.Id == locationId, cancellationToken);
        if (getLocationResult.IsFailure)
        {
            return getLocationResult.Error.ToErrors();
        }

        var location = getLocationResult.Value;

        if (command.PreviewId.HasValue)
        {
            var existResult = await fileService.CheckFileExistAsync(command.PreviewId.Value, IMAGE_MEDIA_TYPE, cancellationToken);
            if (existResult.IsFailure)
            {
                return existResult.Error;
            }

            if (!existResult.Value)
            {
                return GeneralErrors.NotFound("preview").ToErrors();
            }
        }

        location.UpdatePreviewId(command.PreviewId);

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }

        await cacheInvalidator.InvalidateLocationAsync(location.Id.Value, cancellationToken);

        logger.LogInformation(
            "Success update location preview with id: {locationId} preview id: {previewId}",
            location.Id.Value,
            location.PreviewId);

        return UnitResult.Success<Errors>();
    }
}