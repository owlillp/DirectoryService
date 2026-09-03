using CSharpFunctionalExtensions;
using FileService.VideoProcessing.Pipeline;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing;

public class VideoProcessingService(
    ILogger<VideoProcessingService> logger,
    IProcessingPipeline processingPipeline) : IVideoProcessingService
{
    public async Task<UnitResult<Error>> ProcessVideoAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting video processing for asset with id: {videoAssetId}", videoAssetId);

        var pipelineResult = await processingPipeline.ProcessAllStepsAsync(videoAssetId, cancellationToken);
        return pipelineResult;
    }
}