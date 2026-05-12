using DirectoryService.Infrastructure.Postgres.Seeding.Seeders;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Postgres.Seeding;

public static class SeederExtension
{
    public static IServiceCollection AddSeeders(this IServiceCollection services)
    {
        services.AddScoped<ISeeder, DatabaseCleanupSeeder>();
        services.AddScoped<ISeeder, LocationsSeeder>();
        services.AddScoped<ISeeder, PositionsSeeder>();
        services.AddScoped<ISeeder, DepartmentsSeeder>();
        services.AddScoped<ISeeder, DepartmentPositionsSeeder>();

        return services;
    }

    public static async Task<IServiceProvider> RunSeeding(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
        var seeders = scope.ServiceProvider
            .GetServices<ISeeder>()
            .OrderBy(seeder => seeder.Order)
            .ToArray();

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return services;
    }
}