using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Application.Messaging.Publishers;
using FileService.Contracts.Files.Responses;
using FileService.Domain;
using FileService.Domain.Assets;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Commands.StartUpload;

public class StartUploadHandler(
    ILogger<StartUploadHandler> logger,
    IValidator<StartUploadCommand> validator,
    IFileStorageProvider fileStorageProvider,
    IAssetCreatedEventPublisher assetCreatedEventPublisher,
    ITransactionManager transactionManager,
    IMediaAssetRepository repository) : ICommandHandler<StartUploadResponse, StartUploadCommand>
{
    public async Task<Result<StartUploadResponse, Errors>> Handle(StartUploadCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var request = command.Request;

        var fileName = FileName.Create(request.FileName).Value;
        var contentType = ContentType.Create(request.ContentType).Value;
        var mediaData = MediaData.Create(fileName, contentType, request.Size, 1).Value;
        var assetOwner = MediaOwner.Create(request.Context, request.ContextId).Value;
        var mediaAsset = MediaAsset.CreateForUpload(mediaData, request.AssetType.ToAssetType(), assetOwner).Value;

        var addResult = await repository.AddAsync(mediaAsset, cancellationToken);
        if (addResult.IsFailure)
        {
            return addResult.Error.ToErrors();
        }

        var publishResult = await assetCreatedEventPublisher.PublishAsync(mediaAsset);
        if (publishResult.IsFailure)
        {
            return publishResult.Error.ToErrors();
        }

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }

        var generateUrlResult = await fileStorageProvider.GenerateUploadUrlAsync(mediaAsset.UploadKey, mediaData);
        if (generateUrlResult.IsFailure)
        {
            return generateUrlResult.Error.ToErrors();
        }

        logger.LogInformation(
            "Media asset started uploading: {mediaAssetId} with key: {storageKey}",
            mediaAsset.Id,
            mediaAsset.Key);

        return new StartUploadResponse(mediaAsset.Id, generateUrlResult.Value);
    }
}