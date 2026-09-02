using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.ProcessRunner;

public interface IProcessRunner
{
    Task<Result<ProcessResult, Error>> RunAsync(
        ProcessComand command,
        Action<string>? onOutput = null,
        CancellationToken cancellationToken = default);
}