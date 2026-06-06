namespace DirectoryService.Infrastructure.Postgres.BackgroundServices.CleanupService;

public interface ICleanupService
{
    string Name { get; }

    bool Enabled { get; }

    Task<int> CleanupAsync(CancellationToken cancellationToken);
}