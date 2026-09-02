using CSharpFunctionalExtensions;
using FileService.Domain.MediaProcessing;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Pipeline;

public interface IProcessingStepHandler
{
    StepType StepType { get; }

    Task<Result<ProcessingContext, Error>> ExecuteAsync(ProcessingContext context, CancellationToken cancellationToken = default);
}