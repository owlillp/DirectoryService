using CrystalQuartz.AspNetCore;
using Framework.Middlewares;
using Quartz;
using Serilog;

namespace FileService.Presentation.Configuration;

public static class AppConfigurationExtensions
{
    public static async Task<IApplicationBuilder> Configure(this WebApplication app, string[] args)
    {
        app.UseCors(builder =>
        {
            builder.WithOrigins(
                        "http://localhost:3000",
                        "http://localhost:3001",
                        "http://localhost",
                        "http://frontend:3000")
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
        }

        app.UseRouting();
        app.UseAuthorization();
        app.UseCrystalQuartz(() => app.Services.GetRequiredService<ISchedulerFactory>().GetScheduler());
        app.MapControllers();

        return app;
    }
}