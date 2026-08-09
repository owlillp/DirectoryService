using Amazon.S3;
using FileService.Application.Abstractions;
using FileService.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddS3(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileStorageOptions>(configuration.GetSection(nameof(FileStorageOptions)));

        services.AddScoped<IFileStorageProvider, S3Provider>();

        services.AddSingleton<IAmazonS3>(sp =>
        {
            FileStorageOptions fileStorageOptions = sp.GetRequiredService<IOptions<FileStorageOptions>>().Value;

            var config = new AmazonS3Config
            {
                ServiceURL = fileStorageOptions.Endpoint,
                UseHttp = !fileStorageOptions.WithSsl,
                ForcePathStyle = true,
            };
            return new AmazonS3Client(fileStorageOptions.AccessKey, fileStorageOptions.SecretKey, config);
        });

        services.AddTransient<IChunkSizeCalculator, ChunkSizeCalculator>();

        services.AddHostedService<S3BucketInitializationService>();

        return services;
    }
}