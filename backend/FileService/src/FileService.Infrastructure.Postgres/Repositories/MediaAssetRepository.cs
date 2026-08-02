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
            var department = await dbContext.MediaAssets
                .FirstOrDefaultAsync(expression, cancellationToken);

            return department != null
                ? department
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
}