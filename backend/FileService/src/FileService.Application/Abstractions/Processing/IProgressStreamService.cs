using System.Net.ServerSentEvents;
using FileService.Contracts.Processing.Dtos;

namespace FileService.Application.Abstractions.Processing;

public interface IProgressStreamService
{
    IAsyncEnumerable<SseItem<ProgressEventDto>> StreamProgressAsync(Guid videoAssetId, CancellationToken cancellationToken = default);
}