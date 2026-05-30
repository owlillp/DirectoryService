using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Commands.LinkPosition;

public class LinkPositionHandler(
    ILogger<LinkPositionHandler> logger,
    IValidator<LinkPositionCommand> validator,
    IDepartmentsRepository departmentsRepository,
    ITransactionManager transactionManager)
    : ICommandHandler<LinkPositionCommand>
{
    public async Task<UnitResult<Errors>> Handle(LinkPositionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var departmentId = new DepartmentId(command.DepartmentId);
        var positionId = new PositionId(command.PositionId);

        var getResult = await departmentsRepository.GetByIdWithLockAsync(departmentId, cancellationToken);
        if (getResult.IsFailure)
        {
            return getResult.Error.ToErrors();
        }

        var department = getResult.Value;

        if (department.Positions.Any(dl => dl.PositionId == positionId))
        {
            return GeneralErrors
                .Conflict(positionId.Value.ToString(), nameof(LinkPositionCommand.PositionId))
                .ToErrors();
        }

        department.LinkPosition(positionId);

        var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error.ToErrors();
        }

        logger.LogInformation(
            "Success link position with id [{positionId}] to department with id [{departmentId}]",
            positionId.Value,
            departmentId.Value);

        return UnitResult.Success<Errors>();
    }
}