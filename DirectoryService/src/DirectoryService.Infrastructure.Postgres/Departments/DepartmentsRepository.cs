using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Failures;

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
            logger.LogError(ex, "Operation was canceled while creating department with name: {name}", department.Name.Value);
            return GeneralErrors.Canceled("Process create department");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating department with name: {name}", department.Name.Value);
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while deleting locations from department with id: {departmentId}", departmentId.Value);
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<Department, Error>> GetByIdWithLockAsync(DepartmentId departmentId, CancellationToken cancellationToken)
    {
        try
        {
            var department = await dbContext.Departments
                .FromSql($"SELECT * FROM departments WHERE id = {departmentId.Value} FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);

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
        var sql = """
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
        var sql = """
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
}