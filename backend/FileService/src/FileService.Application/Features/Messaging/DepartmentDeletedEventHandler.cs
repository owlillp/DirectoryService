using Core.Abstractions.Database;
using FileService.Application.Abstractions;
using FileService.Application.Messaging.Publishers;
using FileService.Application.Models;
using Messaging.IntegrationEvents.Directories.Events;
using Microsoft.Extensions.Logging;

namespace FileService.Application.Features.Messaging;

public class DepartmentDeletedEventHandler(
    ILogger<DepartmentDeletedEventHandler> logger,
    IMediaAssetRepository mediaAssetRepository,
    IFileStorageProvider fileStorageProvider,
    MediaAssetCacheInvalidator cacheInvalidator,
    IAssetDeletedEventPublisher assetDeletedEventPublisher,
    ITransactionManager transactionManager)
{
    public async Task Handle(DepartmentDeletedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received DepartmentDeletedEvent for Department: {departmentId}", @event.DepartmentId);

        var getMediaAssetResult = await mediaAssetRepository.GetByAsync(
            ma => ma.Owner.EntityId == @event.DepartmentId,
            cancellationToken);

        if (getMediaAssetResult.IsFailure)
        {
            return;
        }

        var mediaAsset = getMediaAssetResult.Value;

        var deleteResult = await fileStorageProvider.DeleteFileAsync(mediaAsset.UploadKey, cancellationToken);
        if (deleteResult.IsFailure && deleteResult.Error.Code != "object.not.found")
        {
            logger.LogInformation("Failed to delete file: {fileId}", mediaAsset.Id);
            return;
        }

        mediaAsset.MarkDeleted();

        var publishResult = await assetDeletedEventPublisher.PublishAsync(mediaAsset);
        if (publishResult.IsFailure)
        {
            return;
        }

        var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return;
        }

        await cacheInvalidator.InvalidateMediaAssetAsync(mediaAsset.Id, mediaAsset.Key, cancellationToken);

        logger.LogInformation(
            "Delete file: {fileId} with key: {key} successful",
            mediaAsset.Id,
            mediaAsset.Key);
    }
}