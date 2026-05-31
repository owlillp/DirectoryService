using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Departments.Failures;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Commands.SoftDeleteDepartment;

public class SoftDeleteDepartmentHandler(
    ILogger<SoftDeleteDepartmentHandler> logger,
    IValidator<SoftDeleteDepartmentCommand> validator,
    ITransactionManager transactionManager,
    IDepartmentsRepository departmentsRepository)
    : ICommandHandler<SoftDeleteDepartmentCommand>
{
    public async Task<UnitResult<Errors>> Handle(SoftDeleteDepartmentCommand command, CancellationToken cancellationToken)
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

        var destinationPath = department.Path;

        department.Deactivate();

        var lockDescendantsResult = await departmentsRepository.LockDescendantsAsync(destinationPath, cancellationToken);
        if (lockDescendantsResult.IsFailure)
        {
            transactionScope.Rollback();
            return lockDescendantsResult.Error.ToErrors();
        }

        var updateDescendantsPathResult = await departmentsRepository.UpdateDescendantsPathAsync(destinationPath, department.Path, cancellationToken);
        if (updateDescendantsPathResult.IsFailure)
        {
            transactionScope.Rollback();
            return updateDescendantsPathResult.Error.ToErrors();
        }

        var deactivateUnusedReferencesResult = await departmentsRepository.DeactivateUnusedReferencesAsync(departmentId, cancellationToken);
        if (deactivateUnusedReferencesResult.IsFailure)
        {
            transactionScope.Rollback();
            return deactivateUnusedReferencesResult.Error.ToErrors();
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

        logger.LogInformation("Success soft delete department with id [{departmentId}]", departmentId.Value);

        return UnitResult.Success<Errors>();
    }
}