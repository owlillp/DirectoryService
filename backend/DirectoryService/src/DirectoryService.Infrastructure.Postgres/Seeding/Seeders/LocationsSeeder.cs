using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.Seeding.Seeders;

public class LocationsSeeder(DirectoryServiceDbContext dbContext, ILogger<LocationsSeeder> logger) : ISeeder
{
    public int Order => SeedingConstants.LOCATIONS_ORDER;

    public async Task SeedAsync()
    {
        logger.LogInformation("Start seeding locations...");
        foreach (var batch in Enumerable.Range(0, SeedingConstants.LOCATIONS_COUNT).Chunk(SeedingConstants.BATCH_SIZE))
        {
            var entities = batch.Select(index => CreateLocation(index)).ToList();
            await dbContext.Locations.AddRangeAsync(entities);
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();
        }

        logger.LogInformation("Finish seeding locations");
    }

    private Location CreateLocation(int index)
    {
        var random = Random.Shared;
        string country = SeedingTemplates.Countries[index % SeedingTemplates.Countries.Length];
        string city = SeedingTemplates.Cities[random.Next(SeedingTemplates.Cities.Length)];
        string street = $"{SeedingTemplates.Streets[random.Next(SeedingTemplates.Streets.Length)]} Street";
        int postalCode = 10000 + index;
        int buildingNumber = random.Next(1, 300);
        string timezone = SeedingTemplates.Timezones[random.Next(SeedingTemplates.Timezones.Length)];
        string locationName = $"{city} Office {index + 1}";

        var name = LocationName.Create(locationName).Value;
        var address = LocationAddress.Create(country, city, street, postalCode, buildingNumber).Value;
        var timezoneValue = LocationTimezone.Create(timezone).Value;

        return Location.Create(name, address, timezoneValue);
    }
}