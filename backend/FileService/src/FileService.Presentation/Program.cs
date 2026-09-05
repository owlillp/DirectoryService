using System.Globalization;
using FileService.Infrastructure.Postgres.Initialization;
using FileService.Presentation.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

try
{
    Log.Information("Starting application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddDependency(builder.Configuration);

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var quartzInitializer = scope.ServiceProvider.GetRequiredService<QuartzDbInitializer>();
        await quartzInitializer.InitializeAsync();
    }

    await app.Configure(args);

    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

namespace DirectoryService.Presentation
{
    public partial class Program;
}