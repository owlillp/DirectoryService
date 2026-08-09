using FileService.Domain;
using Microsoft.Extensions.Caching.Hybrid;

namespace FileService.Application.Models;

public class MediaAssetCacheInvalidator(HybridCache cache)
{
    public async Task InvalidateMediaAssetAsync(Guid mediaAssetId, StorageKey? storageKey, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(MediaAssetCacheKeys.BuildItemKey(mediaAssetId), cancellationToken);

        if(storageKey != null)
        {
            await InvalidatePresignedUrlAsync(storageKey, cancellationToken);
        }
    }

    public async Task InvalidatePresignedUrlAsync(StorageKey storageKey, CancellationToken cancellationToken)
        => await cache.RemoveAsync(MediaAssetCacheKeys.BuildPresignedUrlKey(storageKey), cancellationToken);
}