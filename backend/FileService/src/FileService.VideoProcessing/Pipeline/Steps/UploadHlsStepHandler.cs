using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain;
using FileService.Domain.MediaProcessing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Pipeline.Steps;

public class UploadHlsStepHandler(
    ILogger<GenerateHlsStepHandler> logger,
    IOptions<VideoProcessingOptions> options,
    IFileStorageProvider fileStorageProvider) : IProcessingStepHandler
{
    public StepType StepType => StepType.UPLOAD_HLS;

    private readonly VideoProcessingOptions _options = options.Value;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Uploading HLS to S3 for videoAssetId: {videoAssetId}", context.VideoAsset.Id);

        if (string.IsNullOrWhiteSpace(context.HlsOutputDirectory))
        {
            return GeneralErrors.Failure("HLS output directory not specified");
        }

        if (!Directory.Exists(context.HlsOutputDirectory))
        {
            return GeneralErrors.Failure("HLS output directory does not exist");
        }

        string[] hlsFiles = Directory.GetFiles(context.HlsOutputDirectory, "*.*", SearchOption.AllDirectories);
        if (!hlsFiles.Any())
        {
            return GeneralErrors.Failure("No HLS files found in output directory");
        }

        var hlsRootKeyResul = context.VideoAsset.GetHlsRootKey();
        if (hlsRootKeyResul.IsFailure)
        {
            return hlsRootKeyResul.Error;
        }

        using var throttler = new SemaphoreSlim(_options.UploadDegreeOfParallelism);

        var uploadTasks = hlsFiles.Select(async file =>
        {
            await throttler.WaitAsync(cancellationToken);
            try
            {
                return await UploadHlsFileAsync(hlsRootKeyResul.Value, file, cancellationToken);
            }
            finally
            {
                throttler.Release();
            }
        }).ToArray();

        var results = await Task.WhenAll(uploadTasks);

        var firstFailure = results.FirstOrDefault(r => r.IsFailure);
        if (firstFailure.IsFailure)
            return firstFailure.Error;

        logger.LogInformation(
            "Successfully upload {fileCount} HLS files for videoAssetId: {videoAssetId}",
            hlsFiles.Length,
            context.VideoAsset.Id);

        var masterPlaylistKeyResult = context.VideoAsset.GetHlsMasterPlaylistKey();
        if (masterPlaylistKeyResult.IsFailure)
        {
            return masterPlaylistKeyResult.Error;
        }

        var setKeyResult = context.VideoAsset.SetHlsMasterPlaylistKey(masterPlaylistKeyResult.Value);
        if (setKeyResult.IsFailure)
        {
            return setKeyResult.Error;
        }

        return context;
    }

    private async Task<UnitResult<Error>> UploadHlsFileAsync(
        StorageKey hlsRootKey,
        string localFilePath,
        CancellationToken cancellationToken)
    {
        string fileName = Path.GetFileName(localFilePath);

        var storageKeyResult = hlsRootKey.AppendKey(fileName);
        if (storageKeyResult.IsFailure)
        {
            return storageKeyResult.Error;
        }

        string contentType = GetContentType(localFilePath);

        await using FileStream fileStream = File.OpenRead(localFilePath);

        return await fileStorageProvider.UploadFileAsync(
            storageKeyResult.Value,
            fileStream,
            contentType,
            cancellationToken);
    }

    private string GetContentType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".m3u8" => "application/vnd.apple.mpegurl",
            ".ts" => "video/mp2t",
            _ => "application/octet-stream"
        };
    }
}