using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Commands.AbortMultipartUpload;

public class AbortMultipartUploadHandler(
    ILogger<AbortMultipartUploadHandler> logger,
    IValidator<AbortMultipartUploadCommand> validator,
    IMediaAssetRepository repository,
    IFileStorageProvider fileStorageProvider,
    ITransactionManager transactionManager) : ICommandHandler<AbortMultipartUploadCommand>
{
    public async Task<UnitResult<Errors>> Handle(AbortMultipartUploadCommand command, CancellationToken cancellationToken)
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

        var abortResult = await fileStorageProvider.AbortMultipartUploadAsync(mediaAsset.UploadKey, request.UploadId, cancellationToken);
        if (abortResult.IsFailure)
        {
            logger.LogInformation("Failed to abort uploading the media asset {mediaAssetId}", mediaAsset.Id);
            return abortResult.Error.ToErrors();
        }

        mediaAsset.MarkDeleted();

        var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error.ToErrors();
        }

        logger.LogInformation(
            "Abort multipart upload file: {fileId} with key: {key} successful",
            mediaAsset.Id,
            mediaAsset.Key);

        return UnitResult.Success<Errors>();
    }
}