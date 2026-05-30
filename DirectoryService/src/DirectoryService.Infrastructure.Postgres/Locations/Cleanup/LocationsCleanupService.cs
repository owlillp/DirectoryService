using DirectoryService.Infrastructure.Postgres.BackgroundServices.CleanupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Postgres.Locations.Cleanup;

public class LocationsCleanupService(
    ILogger<LocationsCleanupService> logger,
    IOptions<LocationsCleanupOptions> options,
    DirectoryServiceDbContext dbContext)
    : CleanupServiceBase(logger, options.Value)
{
    public override string Name => nameof(LocationsCleanupService);

    protected override async Task<int> CleanupBatchAsync(int thresholdDays, int batchSize, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var locationIds = await dbContext.Locations
                .Where(l => !l.IsActive)
                .OrderBy(l => l.Id)
                .Take(batchSize)
                .Select(l => l.Id)
                .ToListAsync(cancellationToken);

            if (locationIds.Count == 0)
            {
                await transaction.CommitAsync(cancellationToken);
                return 0;
            }

            await dbContext.DepartmentLocations
                .Where(dl => locationIds.Contains(dl.LocationId))
                .ExecuteDeleteAsync(cancellationToken);

            int deletedCount = await dbContext.Locations
                .Where(l => locationIds.Contains(l.Id))
                .ExecuteDeleteAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return deletedCount;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}