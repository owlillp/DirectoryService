using FileService.Domain.MediaProcessing;

namespace FileService.VideoProcessing.Progress;

public interface IVideoProgressReporter
{
    void Publish(VideoProcess process, bool finalize = false);
}