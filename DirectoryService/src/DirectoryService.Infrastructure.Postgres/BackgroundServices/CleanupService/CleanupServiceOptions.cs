namespace DirectoryService.Infrastructure.Postgres.BackgroundServices.CleanupService;

public class CleanupServiceOptions
{
    public bool Enabled { get; set; }

    public int InactiveDaysThreshold { get; set; } = 30;

    public int BatchSize { get; set; } = 100;

    public int ConsecutiveErrorsLimit { get; set; } = 3;

    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);
}