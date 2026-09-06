using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using FileService.Application.Abstractions.Processing;
using FileService.Contracts.Processing.Dtos;

namespace FileService.VideoProcessing.Progress;

public class ProgressStreamService(IProgressEventQueue eventQueue) : IProgressStreamService
{
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(20);

    public async IAsyncEnumerable<SseItem<ProgressEventDto>> StreamProgressAsync(
        Guid videoAssetId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (eventQueue.TryGetLatest(videoAssetId, out var latest))
        {
            yield return new SseItem<ProgressEventDto>(latest);
            if (IsTerminal(latest))
            {
                yield break;
            }
        }

        var reader = eventQueue.GetOrCreateReader(videoAssetId);

        using var timer = new PeriodicTimer(HeartbeatInterval);

        while (!cancellationToken.IsCancellationRequested)
        {
            var waitReadTask = reader.WaitToReadAsync(cancellationToken).AsTask();
            var heartbeatTask = timer.WaitForNextTickAsync(cancellationToken).AsTask();

            var completedTask = await Task.WhenAny(waitReadTask, heartbeatTask).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            if (completedTask == heartbeatTask)
            {
                yield return CreateHeartbeat();
                continue;
            }

            while (reader.TryRead(out var progressEvent))
            {
                yield return new SseItem<ProgressEventDto>(progressEvent);

                if (IsTerminal(progressEvent))
                {
                    yield break;
                }
            }
        }
    }

    private static bool IsTerminal(ProgressEventDto e) =>
        string.Equals(e.ProcessStatus, "completed", StringComparison.OrdinalIgnoreCase)
        || string.Equals(e.ProcessStatus, "failed", StringComparison.OrdinalIgnoreCase);

    private static SseItem<ProgressEventDto> CreateHeartbeat() => new(data: null!, eventType: "ping");

}