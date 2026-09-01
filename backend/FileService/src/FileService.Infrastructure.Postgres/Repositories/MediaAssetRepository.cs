using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace FileService.Infrastructure.Postgres.Repositories;

public class MediaAssetRepository(
    ILogger<MediaAssetRepository> logger,
    FileServiceDbContext dbContext) : IMediaAssetRepository
{
    public async Task<Result<Guid, Error>> AddAsync(MediaAsset asset, CancellationToken cancellationToken)
    {
        try
        {
            dbContext.MediaAssets.Add(asset);

            await dbContext.SaveChangesAsync(cancellationToken);

            return asset.Id;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was canceled while creating media asset");
            return GeneralErrors.Canceled("Process create department");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating media asset");
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<MediaAsset, Error>> GetByAsync(Expression<Func<MediaAsset, bool>> expression, CancellationToken cancellationToken)
    {
        try
        {
            var mediaAsset = await dbContext.MediaAssets
                .FirstOrDefaultAsync(expression, cancellationToken);

            return mediaAsset != null
                ? mediaAsset
                : GeneralErrors.NotFound(nameof(MediaAsset));
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was cancelled while getting media asset");
            return GeneralErrors.Canceled("Process get media asset");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while getting media asset");
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<VideoAsset, Error>> GetVideoByAsync(Expression<Func<VideoAsset, bool>> expression, CancellationToken cancellationToken)
    {
        try
        {
            var videoAsset = await dbContext.MediaAssets.OfType<VideoAsset>()
                .FirstOrDefaultAsync(expression, cancellationToken);

            return videoAsset != null
                ? videoAsset
                : GeneralErrors.NotFound(nameof(VideoAsset));
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was cancelled while getting video asset");
            return GeneralErrors.Canceled("Process get video asset");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while getting video asset");
            return GeneralErrors.Failure();
        }
    }
}