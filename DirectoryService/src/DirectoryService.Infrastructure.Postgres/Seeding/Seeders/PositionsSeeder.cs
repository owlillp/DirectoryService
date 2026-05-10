using DirectoryService.Domain.Positions;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.Seeding.Seeders;

public class PositionsSeeder(DirectoryServiceDbContext dbContext, ILogger<PositionsSeeder> logger) : ISeeder
{
    public int Order => SeedingConstants.POSITIONS_ORDER;

    public async Task SeedAsync()
    {
        logger.LogInformation("Start seeding positions...");

        foreach (var batch in Enumerable.Range(0, SeedingConstants.POSITIONS_COUNT).Chunk(SeedingConstants.BATCH_SIZE))
        {
            var entities = batch.Select(CreatePosition).ToList();
            await dbContext.Positions.AddRangeAsync(entities);
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();
        }

        logger.LogInformation("Finish seeding positions");
    }

    private Position CreatePosition(int index)
    {
        string nameRaw = $"{SeedingTemplates.PositionNames[index % SeedingTemplates.PositionNames.Length]}";
        string descriptionRaw = $"Template role profile {Random.Shared.Next(1000, 9999)}";

        var name = PositionName.Create(nameRaw).Value;
        var description = PositionDescription.Create(descriptionRaw).Value;

        return Position.Create(name, description, []);
    }
}
