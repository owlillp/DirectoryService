using System.Collections.Concurrent;
using System.Threading.Channels;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions.Processing;
using FileService.Contracts.Processing.Dtos;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Progress;

public class InMemoryProgressQueue(ILogger<InMemoryProgressQueue> logger) : IProgressEventQueue
{
    private readonly ConcurrentDictionary<Guid, Channel<ProgressEventDto>> _channels = new();
    private readonly ConcurrentDictionary<Guid, ProgressEventDto> _latestProgress = new();

    public ChannelReader<ProgressEventDto> GetOrCreateReader(Guid videoAssetId)
    {
        var channel = _channels.GetOrAdd(videoAssetId, _ =>
        {
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = false,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
            };
            return Channel.CreateBounded<ProgressEventDto>(options);
        });

        return channel.Reader;
    }

    public UnitResult<Error> TryWriteQueue(ProgressEventDto progressEvent)
    {
        try
        {
            _latestProgress.AddOrUpdate(progressEvent.MediaAssetId, progressEvent, (_, _) => progressEvent);
            if (_channels.TryGetValue(progressEvent.MediaAssetId, out var channel))
            {
                return channel.Writer.TryWrite(progressEvent)
                    ? UnitResult.Success<Error>()
                    : Error.Failure(
                        "progress.queue.full",
                        $"Progress queue if full for MediaAssetId: {progressEvent.MediaAssetId}");
            }

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update latest progress for MediaAssetId: {mediaAssetId}", progressEvent.MediaAssetId);

            return Error.Failure(
                "progress.queue.failure",
                $"Failed to update latest progress for MediaAssetId: {progressEvent.MediaAssetId}");
        }
    }

    public bool TryGetLatest(Guid videoAssetId, out ProgressEventDto progressEvent)
        => _latestProgress.TryGetValue(videoAssetId, out progressEvent!);

    public void Complete(Guid videoAssetId)
    {
        if (_channels.TryRemove(videoAssetId, out var channel))
        {
            channel.Writer.TryComplete();
        }
    }
}