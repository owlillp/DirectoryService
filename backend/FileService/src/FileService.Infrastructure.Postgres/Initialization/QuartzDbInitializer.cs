using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FileService.Infrastructure.Postgres.Initialization;

public class QuartzDbInitializer(
    ILogger<QuartzDbInitializer> logger,
    IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString(Constants.DATABASE_CONNECTION_STRING)
                                                ?? throw new NullReferenceException("Database connection string is null");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string sqlScript = await LoadSqlScriptAsync();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(sqlScript, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);

            logger.LogInformation("Successfully executed quartz db initialization");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize QuartzDb");
            throw;
        }
    }

    private static async Task<string> LoadSqlScriptAsync()
    {
        var assembly = typeof(QuartzDbInitializer).Assembly;
        string resourceName = "FileService.Infrastructure.Postgres.Scripts.quartz_tables_postgres.sql";

        await using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found");
        }

        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync();
    }
}