using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Commands.Delete;

public class DeleteMediaAssetHandler(
    ILogger<DeleteMediaAssetHandler> logger,
    IValidator<DeleteMediaAssetCommand> validator,
    IFileStorageProvider fileStorageProvider,
    ITransactionManager transactionManager,
    IMediaAssetRepository repository) : ICommandHandler<DeleteMediaAssetCommand>
{
    public async Task<UnitResult<Errors>> Handle(DeleteMediaAssetCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var getMediaAssetResult = await repository.GetByAsync(ma => ma.Id == command.FileId, cancellationToken);
        if (getMediaAssetResult.IsFailure)
        {
            return getMediaAssetResult.Error.ToErrors();
        }

        var mediaAsset = getMediaAssetResult.Value;

        var deleteResult = await fileStorageProvider.DeleteFileAsync(mediaAsset.Key, cancellationToken);
        if (deleteResult.IsFailure && deleteResult.Error.Code != "object.not.found")
        {
            logger.LogInformation("Failed to delete file: {fileId}", command.FileId);
            return deleteResult.Error.ToErrors();
        }

        mediaAsset.MarkDeleted();

        var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error.ToErrors();
        }

        logger.LogInformation(
            "Delete file: {fileId} with key: {key} successful",
            mediaAsset.Id,
            mediaAsset.Key);

        return UnitResult.Success<Errors>();
    }
}