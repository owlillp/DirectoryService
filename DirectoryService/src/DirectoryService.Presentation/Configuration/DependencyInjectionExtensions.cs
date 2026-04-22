using System.Text.Json.Serialization;
using DirectoryService.Application;
using DirectoryService.Infrastructure.Postgres;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace DirectoryService.Presentation.Configuration;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDependency(IConfiguration configuration)
        {
            services.ConfigureSerilog(configuration);
            services.AddControllers();
            services.AddOpenApi();
            services.ConfigureApiBehaviorOptions();
            services.ConfigureJsonOptions();

            services.AddApplication();
            services.AddInfrastructurePostgres(configuration);

            return services;
        }

        private IServiceCollection ConfigureApiBehaviorOptions()
        {
            services.Configure<ApiBehaviorOptions>(opt =>
            {
                opt.SuppressModelStateInvalidFilter = true;
            });
            return services;
        }

        private IServiceCollection ConfigureJsonOptions()
        {
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            return services;
        }

        private IServiceCollection ConfigureSerilog(IConfiguration configuration)
        {
            services.AddSerilog((sp, lc) => lc
                 .ReadFrom.Configuration(configuration)
                 .ReadFrom.Services(sp)
                 .Enrich.FromLogContext()
                 .Enrich.WithProperty("ServiceName", "DirectoryService"));

            return services;
        }
    }
}