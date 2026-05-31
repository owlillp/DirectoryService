using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Commands.UnlinkPosition;

public class UnlinkPositionHandler(
    ILogger<UnlinkPositionHandler> logger,
    IValidator<UnlinkPositionCommand> validator,
    IDepartmentsRepository departmentsRepository,
    IReadDbContext readDbContext)
    : ICommandHandler<UnlinkPositionCommand>
{
    public async Task<UnitResult<Errors>> Handle(UnlinkPositionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var departmentId = new DepartmentId(command.DepartmentId);
        var positionId = new PositionId(command.PositionId);

        bool existReference = await readDbContext
            .DepartmentPositionsRead
            .AnyAsync(
                dl => dl.DepartmentId == departmentId && dl.PositionId == positionId,
                cancellationToken);

        if (!existReference)
        {
            return GeneralErrors
                .NotFound(nameof(UnlinkPositionCommand.PositionId), positionId.Value)
                .ToErrors();
        }

        var unlinkResult = await departmentsRepository.UnlinkPositionAsync(departmentId, positionId, cancellationToken);
        if (unlinkResult.IsFailure)
        {
            return unlinkResult.Error.ToErrors();
        }

        logger.LogInformation(
            "Success unlink position with id [{positionId}] to department with id [{departmentId}]",
            positionId.Value,
            departmentId.Value);

        return UnitResult.Success<Errors>();
    }
}