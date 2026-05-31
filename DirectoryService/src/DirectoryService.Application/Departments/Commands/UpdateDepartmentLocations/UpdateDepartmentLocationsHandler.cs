using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Departments.Failures;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartmentLocations;

public class UpdateDepartmentLocationsHandler(
    ILogger<UpdateDepartmentLocationsHandler> logger,
    IValidator<UpdateDepartmentLocationsCommand> validator,
    ITransactionManager transactionManager,
    IDepartmentsRepository departmentsRepository,
    ILocationsRepository locationsRepository
    ) : ICommandHandler<Guid, UpdateDepartmentLocationsCommand>
{
    public async Task<Result<Guid, Errors>> Handle(UpdateDepartmentLocationsCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var transactionScopeResult = await transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error.ToErrors();
        }

        var locationIds = command.Request.LocationIds
            .Select(l => new LocationId(l))
            .ToArray();

        var locationValidationResult = await locationsRepository.ExistAndActiveAsync(locationIds, cancellationToken);
        if (locationValidationResult.IsFailure)
        {
            return locationValidationResult.Error.ToErrors();
        }

        if (!locationValidationResult.Value)
        {
            return GeneralErrors.NotFound(nameof(Location)).ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;

        var departmentId = new DepartmentId(command.DepartmentId);

        var getDepartmentResult = await departmentsRepository.GetByIdWithLockAsync(departmentId, cancellationToken);
        if (getDepartmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return getDepartmentResult.Error.ToErrors();
        }

        var department = getDepartmentResult.Value;

        if (!department.IsActive)
        {
            transactionScope.Rollback();
            return DepartmentsErrors.Inactive(departmentId.Value).ToErrors();
        }

        department.UpdateLocations(locationIds);

        var deleteLocationsResult = await departmentsRepository.DeleteAllLocationsAsync(departmentId, cancellationToken);
        if (deleteLocationsResult.IsFailure)
        {
            transactionScope.Rollback();
            return deleteLocationsResult.Error.ToErrors();
        }

        var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveChangesResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            return commitResult.Error.ToErrors();
        }

        logger.LogInformation("Success updated locations from department with id [{departmentId}]",  departmentId.Value);

        return departmentId.Value;
    }
}