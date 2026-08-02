using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Application.Common;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Commands.CompleteUpload;

public class CompleteUploadHandler(
    ILogger<CompleteUploadHandler> logger,
    IValidator<CompleteUploadCommand> validator,
    ITransactionManager transactionManager,
    IFileStorageProvider fileStorageProvider,
    IMediaAssetRepository repository) : ICommandHandler<CompleteUploadCommand>
{
    public async Task<UnitResult<Errors>> Handle(CompleteUploadCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var transactionScopeResult = await transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;

        var getMediaAssetResult = await repository.GetByAsync(ma => ma.Id == command.FileId, cancellationToken);
        if (getMediaAssetResult.IsFailure)
        {
            return getMediaAssetResult.Error.ToErrors();
        }

        var mediaAsset = getMediaAssetResult.Value;

        var getObjectMetadataResult = await fileStorageProvider.GetAssetMetadataAsync(mediaAsset.Key, cancellationToken);
        if (getObjectMetadataResult.IsFailure)
        {
            mediaAsset.MarkFailed();

            var saveFailedChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
            if (saveFailedChangesResult.IsFailure)
            {
                transactionScope.Rollback();
                return saveFailedChangesResult.Error.ToErrors();
            }

            transactionScope.Commit();

            return getObjectMetadataResult.Error.ToErrors();
        }

        var objectMetadata = getObjectMetadataResult.Value;
        var compareMetaResult = MetadataComparator.Compare(mediaAsset.MediaData, objectMetadata);
        if (compareMetaResult.IsFailure)
        {
            mediaAsset.MarkFailed();

            var saveFailedChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
            if (saveFailedChangesResult.IsFailure)
            {
                transactionScope.Rollback();
                return saveFailedChangesResult.Error.ToErrors();
            }

            transactionScope.Commit();

            return compareMetaResult.Error;
        }

        var setUploaded = mediaAsset.MarkUploaded();
        if (setUploaded.IsFailure)
        {
            transactionScope.Rollback();
            return setUploaded.Error.ToErrors();
        }

        var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveChangesResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            return commitResult.Error.ToErrors();
        }

        logger.LogInformation(
            "Complete upload file: {fileId} with key: {key} successful",
            mediaAsset.Id,
            mediaAsset.Key);

        return UnitResult.Success<Errors>();
    }
}