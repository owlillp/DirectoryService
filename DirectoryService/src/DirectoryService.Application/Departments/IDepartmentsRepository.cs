using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using Shared.Failures;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Result<DepartmentId, Error>> AddAsync(Department department, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdAsync(DepartmentId id, CancellationToken cancellationToken);

    Task<Result<bool, Error>> ExistAndActiveAsync(IEnumerable<DepartmentId> departmentIds, CancellationToken cancellationToken);
}