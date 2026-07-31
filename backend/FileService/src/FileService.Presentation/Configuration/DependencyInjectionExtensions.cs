using FileService.Application;
using FileService.Infrastructure.S3;
using Framework.Logging;
using Framework.Serializations;
using Framework.Swagger;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Presentation.Configuration;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDependency(IConfiguration configuration)
        {
            services.AddCors();
            services.AddControllers();
            services.AddOpenApi("FileServiceApi", "v1");
            services.AddSerilogLogging(configuration, "FileService");
            services.ConfigureApiBehaviorOptions();
            services.AddJsonOptions();

            services.AddApplication();
            services.AddS3(configuration);
            //services.AddInfrastructurePostgres(configuration);

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