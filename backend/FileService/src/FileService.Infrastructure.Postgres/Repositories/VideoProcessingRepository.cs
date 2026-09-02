using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain.MediaProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.Infrastructure.Postgres.Repositories;

public class VideoProcessingRepository(
    ILogger<VideoProcessingRepository> logger,
    FileServiceDbContext dbContext) : IVideoProcessingRepository
{
    public async Task<Result<VideoProcess, Error>> GetByAsync(
        Expression<Func<VideoProcess, bool>> expression,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var videoProcess = await dbContext.VideoProcesses
                .FirstOrDefaultAsync(expression, cancellationToken);

            return videoProcess != null
                ? videoProcess
                : GeneralErrors.NotFound(nameof(VideoProcess));
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was cancelled while getting video process");
            return GeneralErrors.Canceled("Process get video process");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while getting video process");
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<Guid, Error>> AddAsync(
        VideoProcess videoProcess,
        CancellationToken cancellationToken = default)
    {
        try
        {
            dbContext.VideoProcesses.Add(videoProcess);

            return videoProcess.Id;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was canceled while creating video process");
            return GeneralErrors.Canceled("Process create video process");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating video process");
            return GeneralErrors.Failure();
        }
    }
}