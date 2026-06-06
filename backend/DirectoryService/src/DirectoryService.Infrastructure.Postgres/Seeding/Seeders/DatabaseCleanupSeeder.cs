using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.Seeding.Seeders;

public class DatabaseCleanupSeeder(DirectoryServiceDbContext dbContext, ILogger<DatabaseCleanupSeeder> logger) : ISeeder
{
    public int Order => SeedingConstants.CLEANUP_ORDER;

    public async Task SeedAsync()
    {
        logger.LogInformation("Start database cleanup...");

        await dbContext.DepartmentPositions.ExecuteDeleteAsync();
        await dbContext.DepartmentLocations.ExecuteDeleteAsync();
        await dbContext.Departments.ExecuteDeleteAsync();
        await dbContext.Positions.ExecuteDeleteAsync();
        await dbContext.Locations.ExecuteDeleteAsync();

        logger.LogInformation("Finish database cleanup");
    }
}
