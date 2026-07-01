using DirectoryService.Application;
using DirectoryService.Infrastructure.Postgres;
using Framework.Logging;
using Framework.Serializations;
using Framework.Swagger;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Configuration;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDependency(IConfiguration configuration)
        {
            services.AddCors();
            services.AddControllers();
            services.AddOpenApi("DirectoryServiceApi", "v1");
            services.AddSerilogLogging(configuration, "DirectoryService");
            services.ConfigureApiBehaviorOptions();
            services.AddJsonOptions();

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
    }
}