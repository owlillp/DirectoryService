using CSharpFunctionalExtensions;
using FileService.VideoProcessing.FfmpegProcess;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Preview;

public class PreviewGenerator(
    ILogger<PreviewGenerator> logger,
    IOptions<PreviewOptions> options,
    IFfmpegProcessRunner ffmpegProcessRunner) : IPreviewGenerator
{
    private readonly PreviewOptions _options = options.Value;

    public IReadOnlyList<TimeSpan> CalculateExtractionTimes(TimeSpan duration)
    {
        int previewCount = Math.Max((int)(duration.TotalSeconds / _options.SecondsPerPreview), 1);
        previewCount = Math.Min(previewCount, _options.MaxPreviewCount);
        double interval = duration.TotalSeconds / previewCount;

        List<TimeSpan> result = new();
        for (int i = 0; i < previewCount; i++)
        {
            result.Add(TimeSpan.FromSeconds(interval * i));
        }

        return result;
    }

    public async Task<Result<GeneratePreviewDto, Error>> GeneratePreviewAsync(
        string inputFileUrl,
        string workingDirectory,
        IEnumerable<TimeSpan> timestamps,
        CancellationToken cancellationToken)
    {
        string previewDirectory = Path.Combine(workingDirectory, "preview");
        Directory.CreateDirectory(previewDirectory);

        var previewPaths = await ExtractFramesAsync(
            inputFileUrl,
            previewDirectory,
            timestamps,
            cancellationToken);

        if (previewPaths.IsFailure)
        {
            return previewPaths.Error;
        }

        string? spriteSheetPath = null;
        if (previewPaths.Value.Count > 1)
        {
            var createSpriteSheetResult = await CreateSpriteSheetAsync(
                previewPaths.Value,
                previewDirectory,
                cancellationToken);

            if (createSpriteSheetResult.IsSuccess)
            {
                spriteSheetPath = createSpriteSheetResult.Value;
            }
        }

        return new GeneratePreviewDto(previewPaths.Value, spriteSheetPath);
    }

    private async Task<Result<IReadOnlyList<string>, Error>> ExtractFramesAsync(
        string inputFileUrl,
        string previewDirectory,
        IEnumerable<TimeSpan> extractionTimes,
        CancellationToken cancellationToken)
    {
        var previewPaths = new List<string>();
        var timestamps = extractionTimes.ToArray();
        for (int i = 0; i < timestamps.Length; i++)
        {
            string fileName = _options.FileNamePattern.Replace("{index}", i.ToString());
            string outputPath = Path.Combine(previewDirectory, fileName);

            var result = await ffmpegProcessRunner.ExtractFrameAsync(
                inputFileUrl,
                outputPath,
                timestamps[i],
                cancellationToken);

            if (result.IsFailure)
            {
                logger.LogError("Failed to extract frame at {timestamp}s: {error}", timestamps[i].TotalSeconds, result.Error);
                return result.Error;
            }

            previewPaths.Add(outputPath);
        }

        return previewPaths;
    }

    private async Task<Result<string?, Error>> CreateSpriteSheetAsync(
        IEnumerable<string> previewPaths,
        string previewDirectory,
        CancellationToken cancellationToken)
    {
        string spriteSheetPath = Path.Combine(previewDirectory, _options.SpriteSheetFileName);

        var spriteSheetResult = await ffmpegProcessRunner.CreateSpriteSheetAsync(
            previewPaths,
            spriteSheetPath,
            cancellationToken);

        if (spriteSheetResult.IsFailure)
        {
            logger.LogWarning("Failed to create sprite sheet: {error}", spriteSheetResult.Error);
            return spriteSheetResult.Error;
        }

        return spriteSheetPath;
    }
}