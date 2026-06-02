using System.Data.Common;
using DirectoryService.Application.Abstractions;
using DirectoryService.Infrastructure.Postgres;
using DirectoryService.Infrastructure.Postgres.BackgroundServices.CleanupService;
using DirectoryService.Infrastructure.Postgres.Departments.Cleanup;
using DirectoryService.Infrastructure.Postgres.Locations.Cleanup;
using DirectoryService.Infrastructure.Postgres.Positions.Cleanup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class IntegrationTestsWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres")
        .WithDatabase("directory_service_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        await InitializeRespawnerAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();

        await _dbConnection.CloseAsync();
        await _dbConnection.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
        => await _respawner.ResetAsync(_dbConnection);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var backgroundServiceDescriptor = services.FirstOrDefault(s =>
                s.ServiceType == typeof(IHostedService) &&
                s.ImplementationType == typeof(BackgroundCleanupService));
            if (backgroundServiceDescriptor != null)
            {
                services.Remove(backgroundServiceDescriptor);
            }

            services.RemoveAll<DirectoryServiceDbContext>();
            services.AddScoped<DirectoryServiceDbContext>(_ =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<DirectoryServiceDbContext>();
                optionsBuilder.UseNpgsql(_dbContainer.GetConnectionString());
                return new DirectoryServiceDbContext(optionsBuilder.Options);
            });

            services.RemoveAll<IReadDbContext>();
            services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<DirectoryServiceDbContext>());

            services.RemoveAll<IOptions<DepartmentsCleanupOptions>>();
            services.AddOptions<DepartmentsCleanupOptions>()
                .Configure(options =>
                {
                    options.InactiveDaysThreshold = 30;
                    options.BatchSize = 1000;
                    options.RetryDelay = TimeSpan.FromSeconds(5);
                });

            services.RemoveAll<IOptions<LocationsCleanupOptions>>();
            services.AddOptions<LocationsCleanupOptions>()
                .Configure(options =>
                {
                    options.InactiveDaysThreshold = 30;
                    options.BatchSize = 1000;
                    options.RetryDelay = TimeSpan.FromSeconds(5);
                });

            services.RemoveAll<IOptions<PositionsCleanupOptions>>();
            services.AddOptions<PositionsCleanupOptions>()
                .Configure(options =>
                {
                    options.InactiveDaysThreshold = 30;
                    options.BatchSize = 1000;
                    options.RetryDelay = TimeSpan.FromSeconds(5);
                });

            services.RemoveAll<DepartmentsCleanupService>();
            services.AddScoped<DepartmentsCleanupService>(sp =>
                sp.GetRequiredService<IEnumerable<ICleanupService>>()
                    .OfType<DepartmentsCleanupService>()
                    .Single());

            services.RemoveAll<LocationsCleanupService>();
            services.AddScoped<LocationsCleanupService>(sp =>
                sp.GetRequiredService<IEnumerable<ICleanupService>>()
                    .OfType<LocationsCleanupService>()
                    .Single());

            services.RemoveAll<PositionsCleanupService>();
            services.AddScoped<PositionsCleanupService>(sp =>
                sp.GetRequiredService<IEnumerable<ICleanupService>>()
                    .OfType<PositionsCleanupService>()
                    .Single());
        });
    }

    private async Task InitializeRespawnerAsync()
    {
        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
            });
    }
}