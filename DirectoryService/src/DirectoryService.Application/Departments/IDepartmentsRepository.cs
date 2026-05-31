using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Shared.Failures;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Result<DepartmentId, Error>> AddAsync(Department department, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByAsync(Expression<Func<Department, bool>> expression, CancellationToken cancellationToken);

    Task<Result<bool, Error>> ExistAndActiveAsync(IEnumerable<DepartmentId> departmentIds, CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeleteAllLocationsAsync(DepartmentId departmentId, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdWithLockAsync(DepartmentId departmentId, CancellationToken cancellationToken, bool isActive = true, bool includePositions = false, bool includeLocations = false);

    Task<UnitResult<Error>> LockDescendantsAsync(DepartmentPath rootPath, CancellationToken cancellationToken);

    Task<UnitResult<Error>> UpdateDescendantsPathAsync(DepartmentPath destinationPath, DepartmentPath sourcePath, CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeactivateUnusedReferencesAsync(DepartmentId departmentId, CancellationToken cancellationToken);

    Task<UnitResult<Error>> UnlinkLocationAsync(DepartmentId departmentId, LocationId locationId, CancellationToken cancellationToken);

    Task<UnitResult<Error>> UnlinkPositionAsync(DepartmentId departmentId, PositionId positionId, CancellationToken cancellationToken);
}