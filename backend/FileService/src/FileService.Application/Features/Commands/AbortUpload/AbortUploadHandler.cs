using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Commands.AbortUpload;

public class AbortUploadHandler(
    ILogger<AbortUploadHandler> logger,
    IValidator<AbortUploadCommand> validator,
    IFileStorageProvider fileStorageProvider,
    ITransactionManager transactionManager,
    IMediaAssetRepository repository) : ICommandHandler<AbortUploadCommand>
{
    public async Task<UnitResult<Errors>> Handle(AbortUploadCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var getMediaAssetResult = await repository.GetByAsync(ma => ma.Id == command.FileId && ma.Status == MediaStatus.UPLOADING, cancellationToken);
        if (getMediaAssetResult.IsFailure)
        {
            return getMediaAssetResult.Error.ToErrors();
        }

        var mediaAsset = getMediaAssetResult.Value;

        var deleteResult = await fileStorageProvider.DeleteFileAsync(mediaAsset.UploadKey, cancellationToken);
        if (deleteResult.IsFailure && deleteResult.Error.Code != "object.not.found")
        {
            logger.LogInformation("Failed to abort upload file: {fileId}", command.FileId);
            return deleteResult.Error.ToErrors();
        }

        mediaAsset.MarkDeleted();

        var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error.ToErrors();
        }

        logger.LogInformation(
            "Abort upload file: {fileId} with key: {key} successful",
            mediaAsset.Id,
            mediaAsset.Key);

        return UnitResult.Success<Errors>();
    }
}