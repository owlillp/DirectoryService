using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Extensions;

namespace DirectoryService.Infrastructure.Postgres.BackgroundServices.CleanupService;

public class BackgroundCleanupService(
    ILogger<BackgroundCleanupService> logger,
    IServiceScopeFactory scopeFactory,
    IOptions<BackgroundCleanupServiceOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Cleanup background service is disabled");
            return;
        }

        logger.LogInformation("Starting cleanup background service...");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var cleanupServices = scope.ServiceProvider.GetServices<ICleanupService>().ToArray();

                foreach (var service in cleanupServices.Where(cs => cs.Enabled))
                {
                    int deleted = await service.CleanupAsync(cancellationToken);
                    logger.LogInformation("{name} completed with deleted count: {deleted}.", service.Name, deleted);
                }

                var delayInterval = options.Value.ExecutionTime.GetTimeUntil();

                logger.LogInformation(
                    "Cleanup background service completed... Next running: {nextDate}.",
                    DateTime.UtcNow.Add(delayInterval));

                await Task.Delay(delayInterval, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Cleanup background service canceled.");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while executing cleanup background service.");
            }
        }

        logger.LogInformation("Cleanup background service stopped.");
    }
}