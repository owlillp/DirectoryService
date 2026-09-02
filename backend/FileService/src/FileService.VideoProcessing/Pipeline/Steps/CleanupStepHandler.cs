using CSharpFunctionalExtensions;
using FileService.Domain.MediaProcessing;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Pipeline.Steps;

public class CleanupStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.CLEANUP;

    public Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        
    }
}