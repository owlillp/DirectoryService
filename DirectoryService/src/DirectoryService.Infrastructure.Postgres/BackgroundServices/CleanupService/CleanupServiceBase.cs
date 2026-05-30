using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.BackgroundServices.CleanupService;

public abstract class CleanupServiceBase(
    ILogger logger,
    CleanupServiceOptions options)
    : ICleanupService
{
    public bool Enabled => options.Enabled;

    public abstract string Name { get; }

    private int _consecutiveErrors;

    public async Task<int> CleanupAsync(CancellationToken cancellationToken)
    {
        int totalDeleted = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                int batchDeleted = await CleanupBatchAsync(options.InactiveDaysThreshold, options.BatchSize, cancellationToken);
                if (batchDeleted == 0)
                {
                    break;
                }

                totalDeleted += batchDeleted;

                _consecutiveErrors = 0;

                logger.LogDebug(
                    "[{name}] cleanup batch completed. Deleted: {batchDeleted}, Total deleted so far: {totalDeleted}.",
                    Name,
                    batchDeleted,
                    totalDeleted);
            }
            catch (Exception ex)
            {
                _consecutiveErrors++;

                logger.LogError(ex, "[{name}] Error occurred while executing cleanup batch. Try: {try}", Name, _consecutiveErrors);

                if (_consecutiveErrors >= options.ConsecutiveErrorsLimit)
                {
                    logger.LogCritical("[{name}] cleanup stopped after {Count} consecutive errors.", Name, _consecutiveErrors);
                    break;
                }

                await Task.Delay(options.ConsecutiveErrorsLimit, cancellationToken);
            }
        }

        return totalDeleted;
    }

    protected abstract Task<int> CleanupBatchAsync(int thresholdDays, int batchSize, CancellationToken cancellationToken);
}