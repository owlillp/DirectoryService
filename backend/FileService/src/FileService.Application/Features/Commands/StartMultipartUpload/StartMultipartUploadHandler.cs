using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Contracts.Files.Responses;
using FileService.Domain;
using FileService.Domain.Assets;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Commands.StartMultipartUpload;

public class StartMultipartUploadHandler(
    ILogger<StartMultipartUploadHandler> logger,
    IValidator<StartMultipartUploadCommand> validator,
    IMediaAssetRepository repository,
    IFileStorageProvider fileStorageProvider,
    IChunkSizeCalculator chunkSizeCalculator): ICommandHandler<StartMultipartUploadResponse, StartMultipartUploadCommand>
{
    public async Task<Result<StartMultipartUploadResponse, Errors>> Handle(
        StartMultipartUploadCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var request = command.Request;

        var calculateChunkSizeResult = chunkSizeCalculator.Calculate(request.Size);
        if (calculateChunkSizeResult.IsFailure)
        {
            logger.LogInformation("Invalid chunk size: {chunkSizeResult}", calculateChunkSizeResult.Error);
            return calculateChunkSizeResult.Error.ToErrors();
        }

        (int ChunkSize, int TotalChunks) chunkData = calculateChunkSizeResult.Value;

        var fileName = FileName.Create(request.FileName).Value;
        var contentType = ContentType.Create(request.ContentType).Value;
        var mediaData = MediaData.Create(fileName, contentType, request.Size, chunkData.TotalChunks).Value;
        var assetOwner = MediaOwner.Create(request.Context, request.ContextId).Value;
        var mediaAsset = MediaAsset.CreateForUpload(mediaData, request.AssetType.ToAssetType(), assetOwner).Value;

        var addResult = await repository.AddAsync(mediaAsset, cancellationToken);
        if (addResult.IsFailure)
        {
            return addResult.Error.ToErrors();
        }

        var startMultipartUploadResult = await fileStorageProvider.StartMultipartUploadAsync(mediaAsset.UploadKey, mediaAsset.MediaData.ContentType.Value, cancellationToken);
        if (startMultipartUploadResult.IsFailure)
        {
            logger.LogInformation("Failed to start multipart upload: {multipartUploadResult}", startMultipartUploadResult.Error);
            return startMultipartUploadResult.Error.ToErrors();
        }

        string? uploadId = startMultipartUploadResult.Value;

        var generateChunksUploadUrls = await fileStorageProvider.GenerateAllChunksUploadUrlsAsync(mediaAsset.UploadKey, uploadId, chunkData.TotalChunks, cancellationToken);
        if (generateChunksUploadUrls.IsFailure)
        {
            return generateChunksUploadUrls.Error.ToErrors();
        }

        var uploadUrls = generateChunksUploadUrls.Value;

        logger.LogInformation("Successfully generated media asset: {mediaAsset}", mediaAsset.Id);

        return new StartMultipartUploadResponse(
            mediaAsset.Id,
            uploadId,
            uploadUrls,
            chunkData.ChunkSize);
    }
}