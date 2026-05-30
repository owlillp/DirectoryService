using Dapper;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Infrastructure.Postgres.BackgroundServices.CleanupService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Postgres.Departments.Cleanup;

public class DepartmentsCleanupService(
    ILogger<DepartmentsCleanupService> logger,
    IOptions<DepartmentsCleanupOptions> options,
    IDbConnectionFactory connectionFactory)
    : CleanupServiceBase(logger, options.Value)
{
    private const string THRESHOLD_DAYS_PARAMETER = "threshold_days";
    private const string BATCH_SIZE_PARAMETER = "batch_size";

    public override string Name => nameof(DepartmentsCleanupService);

    protected override async Task<int> CleanupBatchAsync(int thresholdDays, int batchSize, CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var parameters = new DynamicParameters();
        parameters.Add(THRESHOLD_DAYS_PARAMETER, thresholdDays);
        parameters.Add(BATCH_SIZE_PARAMETER, batchSize);

        string sql = $"""
                      CREATE TEMP TABLE tmp_delete_candidates AS
                      SELECT d.id,
                             d.parent_id,
                             d.path,
                             d.depth
                      FROM departments d
                      WHERE d.is_active = FALSE
                        AND d.deleted_at IS NOT NULL
                        AND d.deleted_at < NOW() - make_interval(days => @{THRESHOLD_DAYS_PARAMETER})
                      LIMIT @{BATCH_SIZE_PARAMETER};
                      
                      CREATE TEMP TABLE tmp_reparent AS
                      WITH RECURSIVE parents AS (
                          SELECT child.id AS child_id,
                                 child.path AS old_path,
                                 dc.parent_id AS candidates_parent_id
                          FROM departments child
                          JOIN tmp_delete_candidates dc ON child.parent_id = dc.id
                          WHERE child.id NOT IN (SELECT id FROM tmp_delete_candidates)
                          
                          UNION ALL 
                          
                          SELECT p.child_id,
                                 p.old_path,
                                 d.parent_id
                          FROM parents p
                          JOIN departments d ON d.id = p.candidates_parent_id
                          WHERE p.candidates_parent_id IN (SELECT id FROM tmp_delete_candidates)
                      )
                      
                      SELECT DISTINCT ON (child_id)
                          child_id,
                          candidates_parent_id AS new_parent_id,
                          old_path,
                          CASE
                            WHEN candidates_parent_id IS NULL
                            THEN subpath(old_path, nlevel(old_path) - 1)
                            ELSE ( 
                                SELECT p.path || subpath(old_path, nlevel(old_path) - 1)
                                FROM departments p
                                WHERE p.id = candidates_parent_id)
                            END AS new_path
                      FROM parents
                      WHERE candidates_parent_id IS NULL 
                            OR candidates_parent_id NOT IN (SELECT id FROM tmp_delete_candidates);
                            
                      UPDATE departments child
                      SET parent_id = rp.new_parent_id,
                          path = rp.new_path,
                          depth = nlevel(rp.new_path) - 1,
                          updated_at = NOW()
                      FROM tmp_reparent rp
                      LEFT JOIN departments parent ON parent.id = rp.new_parent_id
                      WHERE child.id = rp.child_id;

                      UPDATE departments d
                      SET 
                          path = rp.new_path || subpath(d.path, nlevel(rp.old_path)),
                          depth = nlevel(rp.new_path || subpath(d.path, nlevel(rp.old_path))) - 1,
                          updated_at = NOW()
                      FROM tmp_reparent rp
                      WHERE d.path <@ rp.old_path 
                        AND d.path != rp.old_path;

                      DELETE FROM department_locations
                      WHERE department_id IN (
                            SELECT id 
                            FROM tmp_delete_candidates);

                      DELETE FROM department_positions
                      WHERE department_id IN (
                            SELECT id 
                            FROM tmp_delete_candidates);

                      WITH deleted AS (
                          DELETE FROM departments
                          WHERE id IN (
                              SELECT id 
                              FROM tmp_delete_candidates)
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