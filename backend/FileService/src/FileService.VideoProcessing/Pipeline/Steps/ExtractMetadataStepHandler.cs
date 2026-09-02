using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain.MediaProcessing;
using FileService.VideoProcessing.FfmpegProcess;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Pipeline.Steps;

public class ExtractMetadataStepHandler(
    ILogger<ExtractMetadataStepHandler> logger,
    IFfmpegProcessRunner ffmpegProcessRunner,
    IFileStorageProvider fileStorageProvider) : IProcessingStepHandler
{
    public StepType StepType => StepType.EXTRACT_METADATA;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Extracting metadata from videoAssetId: {videoAssetId}",
            context.VideoAsset.Id);

        var generateUrlResult = await fileStorageProvider.GenerateDownloadUrlAsync(context.VideoAsset.UploadKey, false);
        if (generateUrlResult.IsFailure)
        {
            logger.LogError(
                "Extracting metadata from videoAssetId: {videoAssetId} failed while generating url",
                context.VideoAsset.Id);

            return generateUrlResult.Error;
        }

        var metadataResult = await ffmpegProcessRunner.ExtractMetadataAsync(generateUrlResult.Value, cancellationToken);
        if (metadataResult.IsFailure)
        {
            logger.LogError(
                "Extracting metadata from videoAssetId: {videoAssetId} failed",
                context.VideoAsset.Id);

            return metadataResult.Error;
        }

        context.VideoAsset.SetMetadata(metadataResult.Value);
        return context;
    }
}