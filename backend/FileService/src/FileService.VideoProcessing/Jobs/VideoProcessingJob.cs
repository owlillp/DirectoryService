using FileService.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FileService.VideoProcessing.Jobs;

[DisallowConcurrentExecution]
public class VideoProcessingJob(
    ILogger<VideoProcessingJob> logger,
    IVideoProcessingRepository processingRepository,
    IVideoProcessingService videoProcessingService) : IJob
{
    public static readonly JobKey VideoAssetIdKey = JobKey.Create("VideoAssetId");

    public async Task Execute(IJobExecutionContext context)
    {
        string videoAssetIdStr = context.MergedJobDataMap.GetString(VideoAssetIdKey.Name)!;
        var videoAssetId = new Guid(videoAssetIdStr);

        logger.LogInformation("Starting video processing job for VideoAssetId: {videoAssetId}", videoAssetId);

        var result = await videoProcessingService.ProcessVideoAsync(videoAssetId, context.CancellationToken);
        if (result.IsFailure)
        {
            var getProcessResult = await processingRepository.GetByAsync(
                p => p.VideoAssetId == videoAssetId,
                context.CancellationToken);

            if (getProcessResult.IsFailure)
            {
                logger.LogError(
                        "Failed to get video processing result for VideoAssetId: {videoAssetId}. Process Failed",
                        videoAssetId);

                throw new JobExecutionException(refireImmediately: false);
            }

            var process = getProcessResult.Value;
            if (process.CanRetry())
            {
                var scheduleRetry = process.ScheduleRetry(DateTime.UtcNow);
                if (scheduleRetry.IsFailure)
                {
                    logger.LogError(
                        "Failed to schedule retry Process for VideoAssetId: {videoAssetId} with error: {error}. Retry not possible",
                        videoAssetId,
                        scheduleRetry.Error);
                    throw new JobExecutionException(refireImmediately: false);
                }

                logger.LogInformation(
                    "Video processing for VideoAssetId: {videoAssetId} failed with error: {error}. Next retry start immediately",
                    videoAssetId,
                    result.Error);

                throw new JobExecutionException(refireImmediately: true);
            }

            logger.LogError(
                "Video processing job for VideoAssetId: {videoAssetId} failed with critical error: {error}. Retry not possible",
                videoAssetId,
                result.Error);

            throw new JobExecutionException(refireImmediately: false);
        }
    }
}