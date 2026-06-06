using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Infrastructure.Postgres.Departments;

public class DepartmentsRepository(ILogger<DepartmentsRepository> logger, DirectoryServiceDbContext dbContext)
    : IDepartmentsRepository
{
    public async Task<Result<DepartmentId, Error>> AddAsync(Department department, CancellationToken cancellationToken)
    {
        try
        {
            dbContext.Departments.Add(department);

            await dbContext.SaveChangesAsync(cancellationToken);

            return department.Id;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was canceled while creating department with name [{name}]", department.Name.Value);
            return GeneralErrors.Canceled("Process create department");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating department with name [{name}]", department.Name.Value);
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<Department, Error>> GetByAsync(Expression<Func<Department, bool>> expression, CancellationToken cancellationToken)
    {
        try
        {
            var department = await dbContext.Departments
                .FirstOrDefaultAsync(expression, cancellationToken);

            return department != null
                ? department
                : GeneralErrors.NotFound(nameof(Department));
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was cancelled while getting department");
            return GeneralErrors.Canceled("Process get department");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while getting department");
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<bool, Error>> ExistAndActiveAsync(IEnumerable<DepartmentId> departmentIds, CancellationToken cancellationToken)
    {
        try
        {
            int existCount = await dbContext.Departments
                .CountAsync(
                    d => departmentIds.Contains(d.Id) && d.IsActive,
                    cancellationToken);

            return existCount == departmentIds.Count();
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was cancelled while checking exist department");
            return GeneralErrors.Canceled("Process check exist department");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while check of exist departments");
            return GeneralErrors.Failure();
        }
    }

    public async Task<UnitResult<Error>> DeleteAllLocationsAsync(DepartmentId departmentId, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.DepartmentLocations
                .Where(l => l.DepartmentId == departmentId)
                .ExecuteDeleteAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was cancelled while delete all locations from department with id [{departmentId}]", departmentId.Value);
            return GeneralErrors.Canceled("Process deleting all locations from department");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while deleting locations from department with id [{departmentId}]", departmentId.Value);
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<Department, Error>> GetByIdWithLockAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken,
        bool isActive = true,
        bool includePositions = false,
        bool includeLocations = false)
    {
        string isActiveClause = isActive
            ? "AND d.is_active = TRUE"
            : string.Empty;

        var departmentIdParam = new NpgsqlParameter("departmentId", departmentId.Value);

        string sql = $"""
                      SELECT d.* 
                      FROM departments d 
                      WHERE d.id = @departmentId
                      {isActiveClause}
                      FOR UPDATE
                      """;
        try
        {
            var department = await dbContext.Departments
                .FromSqlRaw(sql, [departmentIdParam])
                .FirstOrDefaultAsync(cancellationToken);

            if (department != null)
            {
                if (includeLocations)
                {
                    await dbContext.Entry(department)
                        .Collection(d => d.Locations)
                        .LoadAsync(cancellationToken);
                }

                if (includePositions)
                {
                    await dbContext.Entry(department)
                        .Collection(d => d.Positions)
                        .LoadAsync(cancellationToken);
                }
            }

            return department != null
                ? department
                : GeneralErrors.NotFound(nameof(Department));
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was cancelled while getting department");
            return GeneralErrors.Canceled("Process get department");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while getting department");
            return GeneralErrors.Failure();
        }
    }

    public async Task<UnitResult<Error>> LockDescendantsAsync(DepartmentPath rootPath, CancellationToken cancellationToken)
    {
        string sql = """
                     SELECT id
                     FROM departments
                     WHERE path <@ @rootPath::ltree
                     AND path != @rootPath::ltree
                     FOR UPDATE
                     """;

        try
        {
            NpgsqlParameter[] parameters = [new ("rootPath", rootPath.Value)];
            await dbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was cancelled while lock descendants from root path: {path}", rootPath.Value);
            return GeneralErrors.Canceled("Process lock descendants");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while lock descendants from root path: {path}", rootPath.Value);
            return GeneralErrors.Failure();
        }
    }

    public async Task<UnitResult<Error>> UpdateDescendantsPathAsync(DepartmentPath destinationPath, DepartmentPath sourcePath, CancellationToken cancellationToken)
    {
        string sql = """
                     UPDATE departments
                     SET 
                         path = @sourcePath::ltree || subpath(path, nlevel(@destinationPath::ltree)),
                         depth = nlevel(@sourcePath::ltree || subpath(path, nlevel(@destinationPath::ltree))) - 1,
                         updated_at = NOW()
                     WHERE path <@ @destinationPath::ltree
                        AND path != @destinationPath::ltree
                     """;

        try
        {
            NpgsqlParameter[] parameters = [
                new ("sourcePath", sourcePath.Value),
                new ("destinationPath", destinationPath.Value)
            ];
            await dbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(
                ex,
                "Operation was cancelled while update descendants path from: {destinationPath} to: {sourcePath}",
                destinationPath.Value,
                sourcePath.Value);

            return GeneralErrors.Canceled("Process lock descendants");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error while while update descendants path from: {destinationPath} to: {sourcePath}",
                destinationPath.Value,
                sourcePath.Value);

            return GeneralErrors.Failure();
        }
    }

    public async Task<UnitResult<Error>> DeactivateUnusedReferencesAsync(DepartmentId departmentId, CancellationToken cancellationToken)
    {
        string sql = """
                     UPDATE locations 
                     SET is_active = FALSE,
                         deleted_at = NOW()
                     WHERE is_active = TRUE
                        AND id IN (
                            SELECT dl.location_id
                            FROM department_locations dl
                            WHERE dl.department_id = @departmentId
                                AND NOT EXISTS (
                                    SELECT 1 FROM department_locations rdl
                                    JOIN departments rd ON rdl.department_id = rd.id
                                    WHERE rdl.location_id = dl.location_id
                                        AND rd.is_active = TRUE
                                        AND rdl.department_id != @departmentId));

                     UPDATE positions 
                     SET is_active = FALSE,
                         deleted_at = NOW()
                     WHERE is_active = TRUE
                        AND id IN (
                            SELECT dp.position_id
                            FROM department_positions dp
                            WHERE dp.department_id = @departmentId
                                AND NOT EXISTS (
                                    SELECT 1 FROM department_positions rdp
                                    JOIN departments rd ON rdp.department_id = rd.id
                                    WHERE rdp.position_id = dp.position_id
                                        AND rd.is_active = TRUE
                                        AND rdp.department_id != @departmentId));
                     """;

        try
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                sql,
                [new NpgsqlParameter("departmentId", departmentId.Value)],
                cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(
                ex,
                "Operation was cancelled while deactivating unused references for department with id [{DepartmentId}]",
                departmentId.Value);
            return GeneralErrors.Canceled("Process deleting all locations from department");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error while deactivating unused references for department with id [{DepartmentId}]",
                departmentId.Value);

            return GeneralErrors.Failure();
        }
    }

    public async Task<UnitResult<Error>> UnlinkLocationAsync(DepartmentId departmentId, LocationId locationId, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext
                .DepartmentLocations
                .Where(dl => dl.DepartmentId == departmentId && dl.LocationId == locationId)
                .ExecuteDeleteAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(
                ex,
                "Operation was cancelled while unlink location with id [{locationId}] from department with id [{DepartmentId}]",
                locationId.Value,
                departmentId.Value);
            return GeneralErrors.Canceled("Process unlink location from department");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error while unlink location with id [{locationId}] from department with id [{DepartmentId}]",
                locationId.Value,
                departmentId.Value);

            return GeneralErrors.Failure();
        }
    }

    public async Task<UnitResult<Error>> UnlinkPositionAsync(DepartmentId departmentId, PositionId positionId, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext
                .DepartmentPositions
                .Where(dl => dl.DepartmentId == departmentId && dl.PositionId == positionId)
                .ExecuteDeleteAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(
                ex,
                "Operation was cancelled while unlink position with id [{positionId}] from department with id [{DepartmentId}]",
                positionId.Value,
                departmentId.Value);
            return GeneralErrors.Canceled("Process unlink position from department");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error while unlink position with id [{positionId}] department with id [{DepartmentId}]",
                positionId.Value,
                departmentId.Value);

            return GeneralErrors.Failure();
        }
    }
}