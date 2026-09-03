using Core.Abstractions.Database;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain.MediaProcessing;
using FileService.VideoProcessing.Pipeline.Steps;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Pipeline;

public class ProcessingPipeline(
    ILogger<ProcessingPipeline> logger,
    IEnumerable<IProcessingStepHandler> handlers,
    IVideoProcessingRepository videoProcessingRepository,
    IMediaAssetRepository mediaAssetRepository,
    ITransactionManager transactionManager
    ) : IProcessingPipeline
{
    public async Task<UnitResult<Error>> ProcessAllStepsAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken = default)
    {
        var contextResult = await LoadContextAsync(videoAssetId, cancellationToken);
        if (contextResult.IsFailure)
        {
            return contextResult.Error;
        }

        var context = contextResult.Value;

        var executeResult = await ExecuteAllStepsAsync(context, cancellationToken);
        if (executeResult.IsFailure)
        {
            return await FinalizeWithFailureAsync(context, executeResult.Error, cancellationToken);
        }

        return await FinalizeAsync(context, cancellationToken);
    }

    private async Task<UnitResult<Error>> ExecuteAllStepsAsync(ProcessingContext context, CancellationToken cancellationToken)
    {
        var videoAssetId = context.VideoAsset.Id;

        while (true)
        {
            var stepResult = context.VideoProcess.ProcessNextStep();
            if (stepResult.IsFailure)
            {
                logger.LogWarning(
                    "Failed to process next step for videoAssetId: {videoAssetId}, status: {status}",
                    videoAssetId,
                    context.VideoProcess.Status);

                return stepResult.Error;
            }

            var currentStep = stepResult.Value;
            if (currentStep == null)
            {
                logger.LogInformation(
                    "All processing steps completed for videoAssetId: {videoAssetId}",
                    videoAssetId);

                return UnitResult.Success<Error>();
            }

            logger.LogInformation(
                "Processing step {stepType} (order: {order}) for videoAssetId: {videoAssetId}",
                currentStep.StepType,
                currentStep.Order,
                videoAssetId);

            var stepHandler = handlers.FirstOrDefault(h => h.StepType == currentStep.StepType);
            if (stepHandler == null)
            {
                string error = $"No handler for step type {currentStep.StepType}";
                logger.LogError("No handler for step type {stepType}", currentStep.StepType);

                context.VideoProcess.FailCurrentStep(error);
                context.VideoProcess.Fail(error, isCriticalError: true);

                var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
                if (saveResult.IsFailure)
                {
                    logger.LogError(
                        "Failed to save context after missing handler for step: {stepType} for videoAssetId: {videoAssetId}",
                        currentStep.StepType,
                        videoAssetId);
                }

                return Error.Failure("pipeline.handler.not.found", error);
            }

            var executionResult = await ExecuteStepSafelyAsync(stepHandler, context, cancellationToken);
            if (executionResult.IsFailure)
            {
                logger.LogError(
                    "Step: {stepType} failed for videoAssetId: {videoAssetId}",
                    currentStep.StepType,
                    videoAssetId);

                context.VideoProcess.FailCurrentStep(executionResult.Error.Message);
                context.VideoProcess.Fail(executionResult.Error.Message, isCriticalError: true);

                var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
                if (saveResult.IsFailure)
                {
                    logger.LogError(
                        "Failed to save context after step failure: {stepType} for videoAssetId: {videoAssetId}",
                        currentStep.StepType,
                        videoAssetId);
                }

                return executionResult.Error;
            }

            context.VideoProcess.CompleteCurrentStep();

            logger.LogInformation(
                "Step: {stepType} completed for videoAssetId: {videoAssetId}. Progress: {progress}%",
                currentStep.StepType,
                videoAssetId,
                context.VideoProcess.ProgressPercentage);

            var completeSaveResult = await transactionManager.SaveChangesAsync(cancellationToken);
            if (completeSaveResult.IsFailure)
            {
                logger.LogError(
                    "Failed to save context after step executing: {stepType} for videoAssetId: {videoAssetId}",
                    currentStep.StepType,
                    videoAssetId);

                return completeSaveResult.Error;
            }
        }
    }

    private async Task<UnitResult<Error>> FinalizeAsync(ProcessingContext context, CancellationToken cancellationToken)
    {
        var videoAssetId = context.VideoAsset.Id;

        var completeAssetResult = context.VideoAsset.CompleteProcessing();
        if (completeAssetResult.IsFailure)
        {
            return completeAssetResult.Error;
        }

        var completeProcessResult = context.VideoProcess.Complete();
        if (completeProcessResult.IsFailure)
        {
            return completeProcessResult.Error;
        }

        logger.LogInformation(
            "Video processing completed successfully for videoAssetId: {videoAssetId}",
            videoAssetId);

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            logger.LogError("Failed to save final state for VideoAssetId: {videoAssetId}", videoAssetId);
            return saveResult.Error;
        }

        return UnitResult.Success<Error>();
    }

    private async Task<UnitResult<Error>> FinalizeWithFailureAsync(
        ProcessingContext context,
        Error error,
        CancellationToken cancellationToken)
    {
        var videoAssetId = context.VideoAsset.Id;

        logger.LogError(
            "Video processing failed for videoAssetId: {videoAssetId}. Error: {error}",
            videoAssetId,
            error);

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            logger.LogError("Failed to save failure state for VideoAssetId: {videoAssetId}", videoAssetId);
            return saveResult.Error;
        }

        return UnitResult.Failure(error);
    }

    private async Task<Result<ProcessingContext, Error>> LoadContextAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken = default)
    {
        var getProcessingResult = await videoProcessingRepository
                .GetByAsync(vp => vp.VideoAssetId == videoAssetId, cancellationToken);

        VideoProcess videoProcess;

        if (getProcessingResult.IsFailure)
        {
            if (getProcessingResult.Error.Type != ErrorType.NOT_FOUND)
            {
                return getProcessingResult.Error;
            }

            videoProcess = new VideoProcess(videoAssetId);

            await videoProcessingRepository.AddAsync(videoProcess, cancellationToken);
            logger.LogInformation("Created new videoProcess for videoAssetId: {videoAssetId}", videoAssetId);
        }
        else
        {
            videoProcess = getProcessingResult.Value;
            logger.LogInformation("Loaded existing videoProcess for videoAssetId: {videoAssetId}", videoAssetId);
        }

        var getVideoAssetResult = await mediaAssetRepository
            .GetVideoByAsync(va => va.Id == videoAssetId, cancellationToken);

        if (getVideoAssetResult.IsFailure)
        {
            return getVideoAssetResult.Error;
        }

        var videoAsset = getVideoAssetResult.Value;
        var startResult = videoAsset.StartProcessing();
        if (startResult.IsFailure)
        {
            return startResult.Error;
        }

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error;
        }

        return new ProcessingContext
        {
            VideoProcess = videoProcess,
            VideoAsset = videoAsset,
        };
    }

    private async Task<Result<ProcessingContext, Error>> ExecuteStepSafelyAsync(
        IProcessingStepHandler handler,
        ProcessingContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            return await handler.ExecuteAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled exception in step handler: {stepType} for videoAssetId: {videoAssetId}",
                handler.StepType,
                context.VideoAsset.Id);

            return Error.Failure("pipeline.step.exception", $"Step execution failed: {ex.Message}");
        }
    }
}