using DirectoryService.Presentation.Middlewares;
using Serilog;

namespace DirectoryService.Presentation.Configuration;

public static class AppConfigurationExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        app.UseExceptionMiddleware();
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUI();
        }

        app.MapControllers();

        return app;
    }
}