using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain.Assets;
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
}