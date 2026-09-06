using FileService.Domain.Assets;
using Quartz;

namespace FileService.Application.Abstractions.Processing;

public interface IProcessingJobFactory
{
    bool CanProcess(MediaAsset mediaAsset);

    IJobDetail CreateJob(MediaAsset mediaAsset);

    ITrigger CreateTrigger(MediaAsset mediaAsset);
}