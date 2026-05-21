using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Extensions;

namespace DirectoryService.Infrastructure.Postgres.Departments.Cleanup;

public class DepartmentsCleanupBackgroundService(
    ILogger<DepartmentsCleanupBackgroundService> logger,
    IServiceScopeFactory scopeFactory,
    IOptions<DepartmentsCleanupOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Departments cleanup background service is disabled");
            return;
        }

        logger.LogInformation("Starting departments cleanup background service...");

        await using var scope = scopeFactory.CreateAsyncScope();
        var cleanupService = scope.ServiceProvider.GetRequiredService<DepartmentsCleanupService>();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                int deletedCount = await cleanupService.CleanupAsync(options.Value.InactiveDaysThreshold, cancellationToken);

                var delayInterval = options.Value.ExecutionTime.GetTimeUntil();

                logger.LogInformation(
                    "Departments cleanup background service completed... Deleted: {count}, Next running: {nextDate}.",
                    deletedCount,
                    DateTime.UtcNow.Add(delayInterval));

                await Task.Delay(delayInterval, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Departments cleanup background service canceled.");

                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while executing departments cleanup background service.");
            }
        }

        logger.LogInformation("Departments cleanup background service stopped.");
    }
}