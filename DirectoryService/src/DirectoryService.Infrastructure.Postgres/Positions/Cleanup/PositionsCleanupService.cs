using DirectoryService.Infrastructure.Postgres.BackgroundServices.CleanupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Postgres.Positions.Cleanup;

public class PositionsCleanupService(
    ILogger<PositionsCleanupService> logger,
    IOptions<PositionsCleanupOptions> options,
    DirectoryServiceDbContext dbContext)
    : CleanupServiceBase(logger, options.Value)
{
    public override string Name => nameof(PositionsCleanupService);

    protected override async Task<int> CleanupBatchAsync(int thresholdDays, int batchSize, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var positionIds = await dbContext.Positions
                .Where(p => !p.IsActive)
                .OrderBy(p => p.Id)
                .Take(batchSize)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            if (positionIds.Count == 0)
            {
                await transaction.CommitAsync(cancellationToken);
                return 0;
            }

            await dbContext.DepartmentPositions
                .Where(dp => positionIds.Contains(dp.PositionId))
                .ExecuteDeleteAsync(cancellationToken);

            int deletedCount = await dbContext.Positions
                .Where(p => positionIds.Contains(p.Id))
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