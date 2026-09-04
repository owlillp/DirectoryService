using CSharpFunctionalExtensions;
using FileService.Domain;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.FfmpegProcess;

public interface IFfmpegProcessRunner
{
    Task<UnitResult<Error>> GenerateHlsAsync(
        string inputFileUrl,
        string outputDirectory,
        CancellationToken cancellationToken = default);

    Task<Result<VideoMetadata, Error>> ExtractMetadataAsync(
        string inputFileUrl,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> ExtractFrameAsync(
        string inputFileUrl,
        string outputPath,
        TimeSpan timestamp,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> CreateSpriteSheetAsync(
        IEnumerable<string> imagePaths,
        string outputPath,
        CancellationToken cancellationToken = default);
}