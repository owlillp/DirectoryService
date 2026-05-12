namespace DirectoryService.Infrastructure.Postgres.Seeding.Seeders;

public interface ISeeder
{
    int Order { get; }

    Task SeedAsync();
}