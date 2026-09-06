using FileService.Application.Abstractions.Processing;
using FileService.Contracts.Processing.Dtos;
using FileService.Domain.MediaProcessing;
using Microsoft.Extensions.Logging;

namespace FileService.VideoProcessing.Progress;

public class VideoProgressReporter(
    ILogger<VideoProgressReporter> logger,
    IProgressEventQueue eventQueue) : IVideoProgressReporter
{
    public void Publish(VideoProcess process, bool finalize = false)
    {
        var progressEvent = CreateEvent(process);
        var enqueueResult = eventQueue.TryWriteQueue(progressEvent);
        if (enqueueResult.IsFailure)
        {
            logger.LogWarning(
                "Write progress event for VideoAssetId: {videoAssetId} failed. Error: {error}",
                process.VideoAssetId,
                enqueueResult.Error);
        }

        if (finalize)
        {
            eventQueue.Complete(process.VideoAssetId);
        }
    }

    private static string NormalizeStatus(ProcessingStatus status)
        => status.ToString().ToLowerInvariant();

    private static string? NormalizeErrorCode(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return null;
        }

        if (error.Trim().Contains('.'))
        {
            return error;
        }

        return null;
    }

    private ProgressEventDto CreateEvent(VideoProcess process)
    {
        string? errorMessage = process.ErrorMessage;
        var failedStep = process.Steps.FirstOrDefault(x => x.Status == StepStatus.FAILED);
        string? error = errorMessage ?? failedStep?.ErrorMessage;

        return new ProgressEventDto
        {
            MediaAssetId = process.VideoAssetId,
            ProcessStatus = NormalizeStatus(process.Status),
            Percent = process.ProgressPercentage,
            StepOrder = process.CurrentStep?.Order ?? -1,
            StepName = process.CurrentStep?.StepType.ToString() ?? null,
            TotalSteps = process.Steps.Count,
            Error = error,
            ErrorCode = NormalizeErrorCode(error),
            PublishedAt = DateTime.UtcNow,
        };
    }
}