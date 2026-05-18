using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure.Postgres.Database;
using DirectoryService.Infrastructure.Postgres.Departments;
using DirectoryService.Infrastructure.Postgres.Departments.Cleanup;
using DirectoryService.Infrastructure.Postgres.Locations;
using DirectoryService.Infrastructure.Postgres.Positions;
using DirectoryService.Infrastructure.Postgres.Seeding;
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
        AddDbContext(services, configuration);

        AddRepositories(services);

        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddSeeders();

        AddBackgroundServices(services);

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        return services;
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        string? dbConnectionString = configuration.GetConnectionString(Constants.DATABASE_CONNECTION_STRING);

        services.AddDbContextPool<DirectoryServiceDbContext>((sp, options) =>
        {
            var hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            options.UseNpgsql(dbConnectionString);
            options.UseLoggerFactory(loggerFactory);

            if (hostEnvironment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        services.AddDbContextPool<IReadDbContext, DirectoryServiceDbContext>((sp, options) =>
        {
            var hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            options.UseNpgsql(dbConnectionString);
            options.UseLoggerFactory(loggerFactory);

            if (hostEnvironment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });
    }

    private static IServiceCollection AddRepositories(IServiceCollection services)
    {
        services.AddScoped<ILocationsRepository, LocationsRepository>();
        services.AddScoped<IPositionsRepository, PositionsRepository>();
        services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();

        return services;
    }

    private static IServiceCollection AddBackgroundServices(IServiceCollection services)
    {
        services.AddOptions<DepartmentsCleanupOptions>()
            .BindConfiguration(Constants.DEPARTMENT_CLEANUP_OPTIONS_SECTION)
            .ValidateOnStart();

        services.AddScoped<DepartmentsCleanupService>();
        services.AddHostedService<DepartmentsCleanupBackgroundService>();

        return services;
    }
}