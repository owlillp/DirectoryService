using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing;

public class VideoProcessingService(ILogger<VideoProcessingService> logger)
{
    public async Task<UnitResult<Error>> ProcessVideoAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting video processing for asset with id: {videoAssetId}", videoAssetId);
        return UnitResult.Success<Error>();
    }
}