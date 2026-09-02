using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain.MediaProcessing;
using FileService.VideoProcessing.FfmpegProcess;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Pipeline.Steps;

public class GenerateHlsStepHandler(
    ILogger<GenerateHlsStepHandler> logger,
    IFfmpegProcessRunner ffmpegProcessRunner,
    IFileStorageProvider fileStorageProvider) : IProcessingStepHandler
{
    public StepType StepType => StepType.GENERATE_HLS;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Generating HLS for videoAssetId: {videoAssetId}", context.VideoAsset.Id);

        string inputFileUrl;
        if (context.MediaAssetUrl != null)
        {
            inputFileUrl = context.MediaAssetUrl;
        }
        else
        {
            logger.LogInformation("Input file url not cached. Generating new presigned URL");

            var generateUrlResult = await fileStorageProvider.GenerateDownloadUrlAsync(context.VideoAsset.UploadKey, false);
            if (generateUrlResult.IsFailure)
            {
                logger.LogError(
                    "Generating HLS from videoAssetId: {videoAssetId} failed while generating url",
                    context.VideoAsset.Id);

                return generateUrlResult.Error;
            }

            inputFileUrl = generateUrlResult.Value;
        }

        if (string.IsNullOrWhiteSpace(context.WorkingDirectory))
        {
            return GeneralErrors.Failure("Working directory not specified");
        }

        if (string.IsNullOrWhiteSpace(context.HlsOutputDirectory))
        {
            return GeneralErrors.Failure("HLS output directory not specified");
        }

        if (context.VideoAsset.Metadata == null)
        {
            logger.LogWarning("Metadata is null, progress tracking will be disabled");
        }

        var generateHlsResult = await ffmpegProcessRunner.GenerateHlsAsync(inputFileUrl, context.HlsOutputDirectory, cancellationToken);
        if (generateHlsResult.IsFailure)
        {
            logger.LogError(
                "Generating HLS from videoAssetId: {videoAssetId} failed",
                context.VideoAsset.Id);

            return generateHlsResult.Error;
        }

        return context;
    }
}