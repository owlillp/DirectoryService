using System.Data.Common;
using Amazon.S3;
using FileService.Application.Abstractions;
using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.S3;
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
using Testcontainers.Minio;
using Testcontainers.PostgreSql;

namespace FileService.IntegrationTests.Infrastructure;

public class IntegrationTestsWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres")
        .WithDatabase("file_service_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly MinioContainer _minioContainer = new MinioBuilder("minio/minio:latest")
        .WithUsername("minioadmin")
        .WithPassword("minioadmin")
        .Build();

    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _minioContainer.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();

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

        await _minioContainer.StopAsync();
        await _minioContainer.DisposeAsync();

        await _dbConnection.CloseAsync();
        await _dbConnection.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
        => await _respawner.ResetAsync(_dbConnection);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var s3InitializationDescriptor = services.FirstOrDefault(s =>
                s.ServiceType == typeof(IHostedService) &&
                s.ImplementationType == typeof(S3BucketInitializationService));
            if (s3InitializationDescriptor != null)
            {
                services.Remove(s3InitializationDescriptor);
            }

            string endpoint = $"http://{_minioContainer.Hostname}:{_minioContainer.GetMappedPublicPort(9000)}";

            services.RemoveAll<IAmazonS3>();
            services.AddSingleton<IAmazonS3>(_ =>
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = endpoint,
                    UseHttp = true,
                    ForcePathStyle = true,
                };
                return new AmazonS3Client("minioadmin", "minioadmin", config);
            });

            services.RemoveAll<IOptions<S3Options>>();
            services.AddSingleton<IOptions<S3Options>>(new OptionsWrapper<S3Options>(new S3Options
            {
                Endpoint = endpoint,
                AccessKey = "minioadmin",
                SecretKey = "minioadmin",
                WithSsl = false,
                DownloadExpirationHours = 24,
                UploadExpirationMinutes = 60,
                MaxConcurrentRequests = 20,
                // S3 (and MinIO) require every multipart part (except the last) to be
                // at least 5MB. Keep the recommended chunk size above that limit so
                // a few MB-sized file splits into valid uploadable parts.
                RecommendedChunkSizeBytes = 5 * 1024 * 1024,
                RequiredBuckets = ["videos"],
            }));

            services.RemoveAll<FileServiceDbContext>();
            services.AddScoped<FileServiceDbContext>(_ =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<FileServiceDbContext>();
                optionsBuilder.UseNpgsql(_dbContainer.GetConnectionString());
                return new FileServiceDbContext(optionsBuilder.Options);
            });

            services.RemoveAll<IReadDbContext>();
            services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<FileServiceDbContext>());
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

