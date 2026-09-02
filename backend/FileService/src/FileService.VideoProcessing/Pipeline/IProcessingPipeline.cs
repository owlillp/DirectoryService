using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Pipeline;

public interface IProcessingPipeline
{
    Task<UnitResult<Error>> ProcessAllStepsAsync(Guid videoAssetId, CancellationToken cancellationToken = default);
}