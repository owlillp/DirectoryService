using System.Diagnostics;
using System.Text;
using CSharpFunctionalExtensions;
using FileService.Domain;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.ProcessRunner;

public class ProcessRunner(ILogger<ProcessRunner> logger) : IProcessRunner
{
    public async Task<Result<ProcessResult, Error>> RunAsync(
        ProcessCommand command,
        Action<string>? onOutput = null,
        CancellationToken cancellationToken = default)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = command.ExecutableFile,
            Arguments = command.Arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data == null)
            {
                return;
            }

            outputBuilder.AppendLine(args.Data);
            onOutput?.Invoke(args.Data);
        };

        process.ErrorDataReceived += (_, error) =>
        {
            if (error.Data == null)
            {
                return;
            }

            errorBuilder.AppendLine(error.Data);
            onOutput?.Invoke(error.Data);
        };

        logger.LogInformation("Starting process: {fileName} {arguments}", command.ExecutableFile, command.Arguments);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Process was canceled: {fileName} {arguments}", command.ExecutableFile, command.Arguments);
            return GeneralErrors.Canceled();
        }

        var result = new ProcessResult(process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
        if (result.ExitCode != 0)
        {
            logger.LogError(
                "Process failed: {fileName} {arguments}. ExitCode: {} Error: {error}",
                command.ExecutableFile,
                command.Arguments,
                result.ExitCode,
                result.StandardError);

            return GeneralErrors.Failure("Process failed");
        }

        return result;
    }
}