using Microsoft.Extensions.Logging;
using Quartz;

namespace FileService.VideoProcessing.Jobs;

[DisallowConcurrentExecution]
public class VideoProcessingJob(
    ILogger<VideoProcessingJob> logger,
    IVideoProcessingService videoProcessingService) : IJob
{
    public static readonly JobKey VideoAssetIdKey = JobKey.Create("VideoAssetId");

    public async Task Execute(IJobExecutionContext context)
    {
        var videoAssetId = context.MergedJobDataMap.GetGuid(VideoAssetIdKey.Name);

        logger.LogInformation("Starting video processing job for VideoAssetId: {videoAssetId}", videoAssetId);

        var result = await videoProcessingService.ProcessVideoAsync(videoAssetId, context.CancellationToken);
        if (result.IsFailure)
        {
            logger.LogError(
                "Video processing job for VideoAssetId: {videoAssetId} failed with error: {error}",
                videoAssetId,
                result.Error);

            throw new JobExecutionException(refireImmediately: false);
        }
    }
}