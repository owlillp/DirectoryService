using System.Text.Json.Serialization;
using DirectoryService.Application;
using DirectoryService.Infrastructure.Postgres;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDependency(IConfiguration configuration)
        {
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
    }
}