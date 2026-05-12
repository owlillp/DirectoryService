using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.Seeding.Seeders;

public class DepartmentPositionsSeeder(DirectoryServiceDbContext dbContext, ILogger<DepartmentPositionsSeeder> logger) : ISeeder
{
    public int Order => SeedingConstants.DEPARTMENT_POSITIONS_ORDER;

    public async Task SeedAsync()
    {
        logger.LogInformation("Start seeding department positions...");

        var departments = await dbContext.Departments.AsNoTracking().ToListAsync();

        foreach (var batch in departments.Chunk(SeedingConstants.BATCH_SIZE))
        {
            List<Position> positions = await dbContext.Positions.AsNoTracking().ToListAsync();
            var relations = CreateRelations(batch, positions);

            await dbContext.DepartmentPositions.AddRangeAsync(relations);
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();
        }

        logger.LogInformation("Finish seeding department positions");
    }

    private List<DepartmentPosition> CreateRelations(IReadOnlyCollection<Department> departments, List<Position> positions)
    {
        var result = new List<DepartmentPosition>();

        foreach (Department department in departments)
        {
            int count = Random.Shared.Next(
                SeedingConstants.MIN_POSITIONS_PER_DEPARTMENT,
                SeedingConstants.MAX_POSITIONS_PER_DEPARTMENT + 1);

            var selectedPositions = positions
                .OrderBy(_ => Random.Shared.Next())
                .Take(count);

            result.AddRange(selectedPositions.Select(position => new DepartmentPosition(department.Id, position.Id)));
        }

        return result;
    }
}
