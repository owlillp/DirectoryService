using CSharpFunctionalExtensions;
using FileService.Domain;
using FileService.VideoProcessing.ProcessRunner;
using Microsoft.Extensions.Options;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.FfmpegProcess;

public class FfmpegProcessRunner(
    IOptions<VideoProcessingOptions> options,
    IProcessRunner processRunner) : IFfmpegProcessRunner
{
    private readonly VideoProcessingOptions _options = options.Value;

    public async Task<Result<VideoMetadata, Error>> ExtractMetadataAsync(
        string inputFileUrl,
        CancellationToken cancellationToken = default)
    {
        string arguments = BuildFfprobeArguments(inputFileUrl);
        var command = new ProcessComand(_options.FfprobePath, arguments);

        var processResult = await processRunner.RunAsync(command, cancellationToken: cancellationToken);
        if (processResult.IsFailure)
        {
            return processResult.Error;
        }

        return FfprobeOutputParser.Parse(processResult.Value.StandardOutput);
    }

    private static string BuildFfprobeArguments(string inputFileUrl)
    {
        return $"""
                -v error
                -select_streams v:0
                -show_entries stream=width,height
                -show_entries format=duration
                -of json
                "{inputFileUrl}"
                """;
    }
}