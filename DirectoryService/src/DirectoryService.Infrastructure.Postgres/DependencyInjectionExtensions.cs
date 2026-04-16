using DirectoryService.Application.Locations;
using DirectoryService.Infrastructure.Postgres.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<DirectoryServiceDbContext>((sp, options) =>
        {
            string? connectionString = configuration.GetConnectionString(Constants.DATABASE_CONNECTION_STRING);
            var hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            options.UseNpgsql(connectionString);
            options.UseLoggerFactory(loggerFactory);

            if (hostEnvironment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }

        });

        AddRepositories(services);

        return services;
    }

    private static IServiceCollection AddRepositories(IServiceCollection services)
    {
        services.AddScoped<ILocationsRepository, LocationsRepository>();

        return services;
    }
}