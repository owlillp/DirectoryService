using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

    public async Task<Result<Department, Error>> GetByIdAsync(DepartmentId id, CancellationToken cancellationToken)
    {
        try
        {
            var department = await dbContext.Departments.FindAsync([id], cancellationToken);

            return department != null
                ? department
                : GeneralErrors.NotFound(nameof(Department), id.Value);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was cancelled while getting department with id {id}", id.Value);
            return GeneralErrors.Canceled("Process get department");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while getting department with id {id}", id.Value);
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
}