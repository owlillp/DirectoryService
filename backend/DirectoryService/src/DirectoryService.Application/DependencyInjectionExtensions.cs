using Core.Abstractions;
using DirectoryService.Application.Locations;
using FileService.Contracts.Communication;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
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
        services.AddFileServiceHttpCommunication(configuration);

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

        services.AddScoped<LocationCacheInvalidator>();

        return services;
    }
}