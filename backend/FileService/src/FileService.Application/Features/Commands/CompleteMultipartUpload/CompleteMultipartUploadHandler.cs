using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Application.Models;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Quartz;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Commands.CompleteMultipartUpload;

public class CompleteMultipartUploadHandler(
    ILogger<CompleteMultipartUploadHandler> logger,
    IValidator<CompleteMultipartUploadCommand> validator,
    IFileStorageProvider fileStorageProvider,
    IMediaAssetRepository repository,
    ITransactionManager transactionManager,
    ISchedulerFactory schedulerFactory,
    IEnumerable<IProcessingJobFactory> jobFactories) : ICommandHandler<Guid, CompleteMultipartUploadCommand>
{
    public async Task<Result<Guid, Errors>> Handle(CompleteMultipartUploadCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var request = command.Request;

        var getMediaAssetResult = await repository.GetByAsync(ma => ma.Id == request.FileId, cancellationToken);
        if (getMediaAssetResult.IsFailure)
        {
            return getMediaAssetResult.Error.ToErrors();
        }

        var mediaAsset = getMediaAssetResult.Value;

        if (mediaAsset.MediaData.ExpectedChunksCount != request.PartETags.Count)
        {
            return GeneralErrors.ValueIsInvalid("Amount of eTags is not equal to amount of chunks.").ToErrors();
        }

        var completeResult = await fileStorageProvider.CompleteMultipartUploadAsync(mediaAsset.UploadKey, request.UploadId, request.PartETags, cancellationToken);

        try
        {
            var transactionResult = await transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionResult.IsFailure)
            {
                return transactionResult.Error.ToErrors();
            }

            var transaction = transactionResult.Value;

            if (completeResult.IsFailure)
            {
                logger.LogInformation(
                    "Can`t complete uploading the media asset {mediaAssetId}, asset mark failed",
                    request.FileId);

                mediaAsset.MarkFailed();

                var saveFailedChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
                if (saveFailedChangesResult.IsFailure)
                {
                    return saveFailedChangesResult.Error.ToErrors();
                }

                return completeResult.Error.ToErrors();
            }

            var getObjectMetadataResult = await fileStorageProvider.GetAssetMetadataAsync(mediaAsset.UploadKey, cancellationToken);
            if (getObjectMetadataResult.IsFailure)
            {
                mediaAsset.MarkFailed();

                var saveFailedChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
                if (saveFailedChangesResult.IsFailure)
                {
                    return saveFailedChangesResult.Error.ToErrors();
                }

                return getObjectMetadataResult.Error.ToErrors();
            }

            var objectMetadata = getObjectMetadataResult.Value;
            var compareMetaResult = MetadataComparator.Compare(mediaAsset.MediaData, objectMetadata);
            if (compareMetaResult.IsFailure)
            {
                logger.LogInformation(
                    "Failed multipart upload file: {fileId} with key: {key}, asset mark failed",
                    mediaAsset.Id,
                    mediaAsset.Key);

                mediaAsset.MarkFailed();

                var saveFailedChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
                if (saveFailedChangesResult.IsFailure)
                {
                    return saveFailedChangesResult.Error.ToErrors();
                }

                return compareMetaResult.Error;
            }

            var setUploaded = mediaAsset.MarkUploaded();
            if (setUploaded.IsFailure)
            {
                return setUploaded.Error.ToErrors();
            }

            var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
            if (saveChangesResult.IsFailure)
            {
                return saveChangesResult.Error.ToErrors();
            }

            logger.LogInformation(
            "Complete multipart upload file: {fileId} with key: {key} successful",
            mediaAsset.Id,
            mediaAsset.Key);

            if (mediaAsset.RequiresProcessing())
            {
                var factory = jobFactories.FirstOrDefault(f => f.CanProcess(mediaAsset));
                if (factory == null)
                {
                    logger.LogError("No processing job found for media asset {mediaAssetId}", mediaAsset.Id);
                    return GeneralErrors.Failure("No processing job factory found").ToErrors();
                }

                var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
                var job = factory.CreateJob(mediaAsset);
                var trigger = factory.CreateTrigger(mediaAsset);

                await scheduler.ScheduleJob(job, trigger, cancellationToken);

                logger.LogInformation("Scheduled new processing job for MediaAssetId: {videoAssetId} successful", mediaAsset.Id);
            }
            else
            {
                var markResult = mediaAsset.MarkReady();
                if (markResult.IsFailure)
                {
                    return markResult.Error.ToErrors();
                }

                logger.LogInformation("Media asset {mediaAssetId} does not required processing. Marked as Ready", mediaAsset.Id);
            }

            var saveChangesAfterProcessingResult = await transactionManager.SaveChangesAsync(cancellationToken);
            if (saveChangesAfterProcessingResult.IsFailure)
            {
                return saveChangesAfterProcessingResult.Error.ToErrors();
            }

            transaction.Commit();
            return mediaAsset.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing multipart upload for MediaAssetId: {mediaAssetId}",  mediaAsset.Id);
            return GeneralErrors.Failure("Error completing multipart upload").ToErrors();
        }
    }
}