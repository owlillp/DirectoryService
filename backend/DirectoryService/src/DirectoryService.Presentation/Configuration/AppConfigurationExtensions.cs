using DirectoryService.Infrastructure.Postgres.Seeding;
using Framework.Middlewares;
using Serilog;

namespace DirectoryService.Presentation.Configuration;

public static class AppConfigurationExtensions
{
    private const string SEEDING_KEY = "--seeding";

    public static async Task<IApplicationBuilder> Configure(this WebApplication app, string[] args)
    {
        app.UseCors(builder =>
        {
            builder.WithOrigins("http://localhost:3000", "http://localhost:3001")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });

        app.UseExceptionMiddleware();
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
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