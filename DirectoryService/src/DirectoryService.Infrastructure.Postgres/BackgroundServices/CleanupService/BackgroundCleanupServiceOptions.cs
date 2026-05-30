namespace DirectoryService.Infrastructure.Postgres.BackgroundServices.CleanupService;

public class BackgroundCleanupServiceOptions
{
    public bool Enabled { get; set; }

    public TimeOnly ExecutionTime { get; set; } = new (1, 0);
}