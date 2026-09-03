using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain.MediaProcessing;
using FileService.VideoProcessing.Preview;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Pipeline.Steps;

public class GeneratePreviewStepHandler(
    ILogger<GeneratePreviewStepHandler> logger,
    IPreviewGenerator previewGenerator,
    IFileStorageProvider fileStorageProvider) : IProcessingStepHandler
{
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

        var generatePreviewResult = await previewGenerator.GeneratePreviewAsync();
    }
}