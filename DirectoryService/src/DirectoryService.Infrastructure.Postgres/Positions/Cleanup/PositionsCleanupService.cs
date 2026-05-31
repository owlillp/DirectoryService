using Dapper;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Infrastructure.Postgres.BackgroundServices.CleanupService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Postgres.Positions.Cleanup;

public class PositionsCleanupService(
    ILogger<PositionsCleanupService> logger,
    IOptions<PositionsCleanupOptions> options,
    IDbConnectionFactory connectionFactory)
    : CleanupServiceBase(logger, options.Value)
{
    private const string INACTIVE_DAYS_THRESHOLD_PARAMETER = "threshold_days";
    private const string BATCH_SIZE_PARAMETER = "batch_size";

    public override string Name => nameof(PositionsCleanupService);

    protected override async Task<int> CleanupBatchAsync(int thresholdDays, int batchSize, CancellationToken cancellationToken)
    {
        var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var parameters = new DynamicParameters();
        parameters.Add(INACTIVE_DAYS_THRESHOLD_PARAMETER, thresholdDays);
        parameters.Add(BATCH_SIZE_PARAMETER, batchSize);

        string sql = $"""
                      DELETE FROM department_positions
                      WHERE position_id IN (
                          SELECT p.id
                          FROM positions p
                          WHERE p.is_active = FALSE
                            AND p.deleted_at IS NOT NULL
                            AND p.deleted_at < NOW() - make_interval(days => @{INACTIVE_DAYS_THRESHOLD_PARAMETER})
                          LIMIT @{BATCH_SIZE_PARAMETER}
                      );

                      WITH deleted AS (
                          DELETE FROM positions
                          WHERE id IN (
                              SELECT p.id
                              FROM positions p
                              WHERE p.is_active = FALSE
                                AND p.deleted_at IS NOT NULL
                                AND p.deleted_at < NOW() - make_interval(days => @{INACTIVE_DAYS_THRESHOLD_PARAMETER})
                              LIMIT @{BATCH_SIZE_PARAMETER}
                          )
                          RETURNING id
                      )
                      SELECT COUNT(*)
                      FROM deleted;
                      """;

        try
        {
            int deletedCount = await connection.ExecuteScalarAsync<int>(
                sql,
                parameters,
                transaction);

            transaction.Commit();
            return deletedCount;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}