using Microsoft.Extensions.Caching.Hybrid;

namespace DirectoryService.Application.Locations;

public sealed class LocationCacheInvalidator(HybridCache cache)
{
    public async Task InvalidateLocationAsync(Guid locationId, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(LocationCacheKeys.BuildItemKey(locationId), cancellationToken);
        await InvalidateListsAsync(cancellationToken);
    }

    public async Task InvalidateListsAsync(CancellationToken cancellationToken)
        => await cache.RemoveByTagAsync(LocationCacheKeys.Tags.All, cancellationToken);

    public async Task InvalidateActiveListsAsync(CancellationToken cancellationToken)
        => await cache.RemoveByTagAsync(LocationCacheKeys.Tags.Active, cancellationToken);

    public async Task InvalidateDepartmentListsAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        await cache.RemoveByTagAsync(LocationCacheKeys.Tags.Department(departmentId), cancellationToken);
        await cache.RemoveByTagAsync(LocationCacheKeys.Tags.All, cancellationToken);
    }
}
