using DirectoryService.Infrastructure.Postgres.Seeding;
using DirectoryService.Presentation.Middlewares;
using Serilog;

namespace DirectoryService.Presentation.Configuration;

public static class AppConfigurationExtensions
{
    private const string SEEDING_KEY = "--seeding";

    public static async Task<IApplicationBuilder> Configure(this WebApplication app, string[] args)
    {
        app.UseExceptionMiddleware();
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUI();

            if (args.Contains(SEEDING_KEY))
            {
                await app.Services.RunSeeding();
            }
        }

        app.MapControllers();

        return app;
    }
}