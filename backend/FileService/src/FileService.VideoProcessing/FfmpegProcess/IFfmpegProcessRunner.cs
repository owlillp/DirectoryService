using CSharpFunctionalExtensions;
using FileService.Domain;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.FfmpegProcess;

public interface IFfmpegProcessRunner
{
    // Task<UnitResult<Error>> GenerateHlsAsync(
    //     HlsGenerationContext context,
    //     CancellationToken cancellationToken = default);

    Task<Result<VideoMetadata, Error>> ExtractMetadataAsync(
        string inputFileUrl,
        CancellationToken cancellationToken = default);
}