using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Preview;

public interface IPreviewGenerator
{
    IReadOnlyList<TimeSpan> CalculateExtractionTimes(TimeSpan duration);

    Task<Result<GeneratePreviewDto, Error>> GeneratePreviewAsync(
        string inputFileUrl,
        string workingDirectory,
        IEnumerable<TimeSpan> timestamps,
        CancellationToken cancellationToken);
}