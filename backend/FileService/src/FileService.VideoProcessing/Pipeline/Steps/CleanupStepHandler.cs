using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain.MediaProcessing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Pipeline.Steps;

public class CleanupStepHandler(
    ILogger<CleanupStepHandler> logger,
    IFileStorageProvider fileStorageProvider) : IProcessingStepHandler
{
    public StepType StepType => StepType.CLEANUP;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Cleanup up temporary files for VideoAssetId: {videoAssetId}", context.VideoAsset.Id);

        if (string.IsNullOrWhiteSpace(context.WorkingDirectory))
        {
            logger.LogWarning("Working directory not specified, skipping Cleanup");
            return await Task.FromResult(context);
        }

        var deleteResult = await fileStorageProvider.DeleteFileAsync(context.VideoAsset.RawKey!, cancellationToken);
        if (deleteResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to delete raw file from storage for VideoAssetId: {videoAssetId}. Error: {error}",
                context.VideoAsset.Id,
                deleteResult.Error);
        }
        else
        {
            logger.LogDebug("Raw dile deleted from storage for VideoAssetId: {videoAssetId}", context.VideoAsset.Id);
        }

        try
        {
            if (Directory.Exists(context.WorkingDirectory))
            {
                Directory.Delete(context.WorkingDirectory, recursive: true);
                logger.LogDebug("Working directory deleted successfully: {directory}", context.WorkingDirectory);

                context.Cleanup();
            }
        }
        catch (Exception ex)
        {
           logger.LogWarning(
               ex,
               "Failed to delete working directory: {workingDirectory}. Will be cleaned up later", 
               context.WorkingDirectory);
        }

        return await Task.FromResult(context);
    }
}