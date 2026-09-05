using FileService.Application.Abstractions;
using FileService.Domain.Assets;
using Quartz;

namespace FileService.VideoProcessing.Jobs;

public class VideoProcessingJobFactory : IProcessingJobFactory
{
    private const string JOB_GROUP = "video-processing";

    public bool CanProcess(MediaAsset mediaAsset)
        => mediaAsset is VideoAsset;

    public IJobDetail CreateJob(MediaAsset mediaAsset)
    {
        return JobBuilder.Create<VideoProcessingJob>()
            .WithIdentity($"video-processing-{mediaAsset.Id}", JOB_GROUP)
            .UsingJobData(VideoProcessingJob.VideoAssetIdKey.Name, mediaAsset.Id)
            .StoreDurably(false)
            .RequestRecovery(true)
            .Build();
    }

    public ITrigger CreateTrigger(MediaAsset mediaAsset)
    {
        return TriggerBuilder.Create()
            .WithIdentity($"video-processing-trigger-{mediaAsset.Id}", JOB_GROUP)
            .StartNow()
            .Build();
    }
}