using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Domain.MediaProcessing;
using FileService.VideoProcessing.Preview;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Pipeline.Steps;

public class GeneratePreviewStepHandler(
    ILogger<GeneratePreviewStepHandler> logger,
    IOptions<PreviewOptions> options,
    IPreviewGenerator previewGenerator,
    IFileStorageProvider fileStorageProvider) : IProcessingStepHandler
{
    private readonly PreviewOptions _options = options.Value;

    public StepType StepType => StepType.GENERATE_PREVIEW;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting generate preview for VideoAssetId: {videoAssetId}", context.VideoAsset.Id);

        if (string.IsNullOrWhiteSpace(context.WorkingDirectory))
        {
            return GeneralErrors.Failure("Working directory not specified");
        }

        if (string.IsNullOrWhiteSpace(context.MediaAssetUrl))
        {
            var assetUrlResult = await fileStorageProvider.GenerateDownloadUrlAsync(context.VideoAsset.UploadKey);
            if (assetUrlResult.IsFailure)
            {
                return assetUrlResult.Error;
            }

            context.SetMediaAssetUrl(assetUrlResult.Value);
        }

        var metadata = context.VideoAsset.Metadata;
        if (metadata == null)
        {
            return Error.Failure("preview.metadata.missing", "Video metadata not available");
        }

        var timestamps = previewGenerator.CalculateExtractionTimes(metadata.Duration);
        if (!timestamps.Any())
        {
            logger.LogWarning("No preview timestamps calculated for video duration: {duration}", metadata.Duration);
            return context;
        }

        var generatePreviewResult = await previewGenerator.GeneratePreviewAsync(
            context.MediaAssetUrl!,
            context.WorkingDirectory,
            timestamps,
            cancellationToken);

        if (generatePreviewResult.IsFailure)
        {
            logger.LogError(
                "Generation preview for VideoAssetId: {videoAssetId} failed. Error: {error}",
                context.VideoAsset.Id,
                generatePreviewResult.Error);

            return generatePreviewResult.Error;
        }

        var generateResult = generatePreviewResult.Value;

        var uploadPreviewsResult = await UploadPreviewsAsync(generateResult.PreviewPaths, context.VideoAsset.Id, cancellationToken);
        if (uploadPreviewsResult.IsFailure)
        {
            logger.LogError(
                "Upload previews for VideoAssetId: {videoAssetId} failed. Error: {error}",
                context.VideoAsset.Id,
                uploadPreviewsResult.Error);

            return uploadPreviewsResult.Error;
        }

        StorageKey? spriteSheetKey = null;
        if (!string.IsNullOrWhiteSpace(generateResult.SpriteSheetPath))
        {
            var uploadSpriteSheetResult = await UploadSpriteSheetAsync(generateResult.SpriteSheetPath, context.VideoAsset.Id, cancellationToken);
            if (uploadSpriteSheetResult.IsFailure)
            {
                logger.LogError(
                    "Upload sprite sheet for VideoAssetId: {videoAssetId} failed. Error: {error}",
                    context.VideoAsset.Id,
                    uploadSpriteSheetResult.Error);

                return uploadSpriteSheetResult.Error;
            }

            spriteSheetKey = uploadSpriteSheetResult.Value;
        }

        context.VideoAsset.SetPreviewKeys(uploadPreviewsResult.Value, spriteSheetKey);

        logger.LogInformation(
            "Preview generation completed for video: {VideoAssetId}. Generated {Count} previews, SpriteSheet: {HasSprite}",
            context.VideoProcess.VideoAssetId,
            uploadPreviewsResult.Value.Count,
            spriteSheetKey != null ? "Yes" : "No");

        return context;
    }

    private async Task<Result<List<StorageKey>, Error>> UploadPreviewsAsync(
        IEnumerable<string> previewPaths,
        Guid videoAssetId,
        CancellationToken cancellationToken)
    {
        var previewKeys = new List<StorageKey>();

        foreach (string previewPath in previewPaths)
        {
            string fileName = Path.GetFileName(previewPath);

            var storageKeyResult = StorageKey.Create(PreviewAsset.LOCATION, videoAssetId.ToString(), fileName);
            if (storageKeyResult.IsFailure)
            {
                return storageKeyResult.Error;
            }

            var uploadResult = await UploadFileAsync(
                previewPath,
                storageKeyResult.Value,
                cancellationToken);

            if (uploadResult.IsFailure)
            {
                return uploadResult.Error;
            }

            previewKeys.Add(uploadResult.Value);

            logger.LogDebug("Uploaded preview {fileName} to: {fullPath}", fileName, uploadResult.Value.FullPath);
        }

        return previewKeys;
    }

    private async Task<Result<StorageKey?, Error>> UploadSpriteSheetAsync(
        string spriteSheetPath,
        Guid videoAssetId,
        CancellationToken cancellationToken)
    {
        string spriteFileName = _options.SpriteSheetFileName;

        var storageKeyResult = StorageKey.Create(PreviewAsset.LOCATION, videoAssetId.ToString(), spriteFileName);
        if (storageKeyResult.IsFailure)
        {
            logger.LogWarning("Failed to create storage key for sprite sheet: {error}", storageKeyResult.Error);
            return storageKeyResult.Error;
        }

        var uploadResult = await UploadFileAsync(
            spriteSheetPath,
            storageKeyResult.Value,
            cancellationToken);

        if (uploadResult.IsFailure)
        {
            logger.LogWarning("Failed to upload sprite sheet: {error}", uploadResult.Error);
            return uploadResult.Error;
        }

        logger.LogDebug("Uploaded sprite sheet to: {fullPath}", uploadResult.Value.FullPath);
        return uploadResult.Value;
    }

    private async Task<Result<StorageKey, Error>> UploadFileAsync(
        string filePath,
        StorageKey storageKey,
        CancellationToken cancellationToken)
    {
        var contentTypeResult = ContentType.Create("image/jpeg");
        if (contentTypeResult.IsFailure)
        {
            return contentTypeResult.Error;
        }

        var contentType = contentTypeResult.Value;

        await using var fileStream = File.OpenRead(filePath);
        var uploadResult = await fileStorageProvider.UploadFileAsync(
            storageKey,
            fileStream,
            contentType.Value,
            cancellationToken);

        return uploadResult.IsSuccess
            ? storageKey
            : uploadResult.Error;
    }
}