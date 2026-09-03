using CSharpFunctionalExtensions;
using FileService.Domain.MediaProcessing;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Pipeline.Steps;

public class GeneratePreviewStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.GENERATE_PREVIEW;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(context);
    }
}