using Core.Abstractions;
using FileService.Application.Models;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace FileService.Application;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication(IConfiguration configuration)
        {
            var assembly = typeof(DependencyInjectionExtensions).Assembly;

            services.Scan(scan => scan.FromAssemblies(assembly)
                .AddClasses(classes => classes.AssignableToAny(
                    typeof(ICommandHandler<,>),
                    typeof(ICommandHandler<>),
                    typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.AddValidatorsFromAssembly(assembly);

            services.Configure<CacheOptions>(configuration.GetSection(nameof(CacheOptions)));
            var cacheOptions = configuration.GetSection(nameof(CacheOptions)).Get<CacheOptions>()
                               ?? new CacheOptions();

            if (cacheOptions.EnableRedisCache)
            {
                services.AddStackExchangeRedisCache(setup =>
                {
                    setup.Configuration = configuration.GetConnectionString("Redis");
                });
            }

            services.AddHybridCache(options =>
            {
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    LocalCacheExpiration = TimeSpan.FromMinutes(cacheOptions.LocalCacheExpirationMinutes),
                    Expiration = TimeSpan.FromMinutes(cacheOptions.CacheExpirationMinutes),
                };
            });

            services.AddScoped<MediaAssetCacheInvalidator>();

            services.AddQuartzServices(configuration);

            return services;
        }

        private IServiceCollection AddQuartzServices(IConfiguration configuration)
        {
            services.AddQuartz(options =>
            {
                options.UsePersistentStore(persistenceOptions =>
                {
                    persistenceOptions.UsePostgres(cfg =>
                    {
                        cfg.ConnectionString = configuration.GetConnectionString("FileServiceDb")
                                               ?? throw new NullReferenceException("Database connection string is null");
                    });

                    persistenceOptions.UseNewtonsoftJsonSerializer();
                    persistenceOptions.UseProperties = true;
                });
            });

            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            return services;
        }
    }
}