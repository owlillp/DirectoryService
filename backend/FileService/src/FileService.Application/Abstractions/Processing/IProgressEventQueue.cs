using System.Threading.Channels;
using CSharpFunctionalExtensions;
using FileService.Contracts.Processing.Dtos;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Abstractions.Processing;

public interface IProgressEventQueue
{
    ChannelReader<ProgressEventDto> GetOrCreateReader(Guid videoAssetId);

    UnitResult<Error> TryWriteQueue(ProgressEventDto progressEvent);

    bool TryGetLatest(Guid videoAssetId, out ProgressEventDto progressEvent);

    void Complete(Guid videoAssetId);
}