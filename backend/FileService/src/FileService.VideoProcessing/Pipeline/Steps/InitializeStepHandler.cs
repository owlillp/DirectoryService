using CSharpFunctionalExtensions;
using FileService.Domain.MediaProcessing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Pipeline.Steps;

public class InitializeStepHandler(ILogger<InitializeStepHandler> logger) : IProcessingStepHandler
{
    public StepType StepType => StepType.INITIALIZE;

    public Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Initialize video processing for videoAssetId: {videoAssetId}", context.VideoAsset.Id);

        var createDirectoryResult = context.CreateWorkingDirectory();
        if (createDirectoryResult.IsFailure)
        {
            return Task.FromResult(Result.Failure<ProcessingContext, Error>(createDirectoryResult.Error));
        }

        logger.LogInformation(
            "Success create working directory: {workingDirectory} for videoAssetId: {videoAssetId}",
            context.WorkingDirectory,
            context.VideoAsset.Id);

        return Task.FromResult(Result.Success<ProcessingContext, Error>(context));
    }
}