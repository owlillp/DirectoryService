using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentHandler(
    ILogger<UpdateDepartmentHandler> logger,
    IValidator<UpdateDepartmentCommand> validator,
    IDepartmentsRepository departmentsRepository,
    ITransactionManager transactionManager)
    : ICommandHandler<UpdateDepartmentCommand>
{
    public async Task<UnitResult<Errors>> Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var departmentId = new DepartmentId(command.DepartmentId);
        var departmentName = DepartmentName.Create(command.Request.Name).Value;

        var getResult = await departmentsRepository.GetByAsync(d => d.Id == departmentId, cancellationToken);
        if (getResult.IsFailure)
        {
            return getResult.Error.ToErrors();
        }

        var department = getResult.Value;
        var destinationName = department.Name;

        department.Rename(departmentName);

        var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error.ToErrors();
        }

        logger.LogInformation(
            "Success update department with id [{PositionId}] : {destinationPositionName} => {NewPositionName}",
            departmentId,
            destinationName.Value,
            departmentName.Value);

        return UnitResult.Success<Errors>();
    }
}