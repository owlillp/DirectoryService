namespace DirectoryService.Infrastructure.Postgres.Departments.Cleanup;

public class DepartmentsCleanupOptions
{
    public bool Enabled { get; set; }

    public int InactiveDaysThreshold { get; set; } = 30;

    public TimeOnly ExecutionTime { get; set; } = new (1, 0);
}