using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Departments.Failures;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace DirectoryService.Application.Departments.UpdateDepartmentParent;

public class UpdateDepartmentParentHandler(
    ILogger<UpdateDepartmentParentHandler> logger,
    IValidator<UpdateDepartmentParentCommand> validator,
    ITransactionManager transactionManager,
    IDepartmentsRepository departmentsRepository
    ) : ICommandHandler<UpdateDepartmentParentCommand>
{
    public async Task<UnitResult<Errors>> Handle(UpdateDepartmentParentCommand command, CancellationToken cancellationToken)
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

        var transactionScope = transactionScopeResult.Value;

        var departmentId = new DepartmentId(command.DepartmentId);
        var getDepartmentResult = await departmentsRepository.GetByIdWithLockAsync(departmentId, cancellationToken);
        if (getDepartmentResult.IsFailure)
        {
            return getDepartmentResult.Error.ToErrors();
        }

        var department = getDepartmentResult.Value;
        var destinationPath = department.Path;

        if (!department.IsActive)
        {
            transactionScope.Rollback();
            return DepartmentsErrors.Inactive(departmentId.Value).ToErrors();
        }

        Department? parentDepartment = null;

        if (command.Request.ParentId.HasValue)
        {
            var parentDepartmentId = new DepartmentId(command.Request.ParentId.Value);
            var getParentResult = await departmentsRepository.GetByIdWithLockAsync(parentDepartmentId, cancellationToken);
            if (getParentResult.IsFailure)
            {
                transactionScope.Rollback();
                return getParentResult.Error.ToErrors();
            }

            parentDepartment = getParentResult.Value;

            if (!parentDepartment.IsActive)
            {
                transactionScope.Rollback();
                return DepartmentsErrors.Inactive(parentDepartmentId.Value).ToErrors();
            }

            if (parentDepartment.Path.StartWith(destinationPath))
            {
                transactionScope.Rollback();
                return DepartmentsErrors.CyclicHierarchy().ToErrors();
            }
        }

        department.UpdateParent(parentDepartment);

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

        if (parentDepartment == null)
        {
            logger.LogInformation(
                "Success updated parent from department with id:{departmentId} to root",
                departmentId.Value);
        }
        else
        {
            logger.LogInformation(
                "Success updated parent from department with id:{departmentId} to parent id: {parentId} ",
                departmentId.Value,
                parentDepartment.Id.Value);
        }

        return UnitResult.Success<Errors>();
    }
}