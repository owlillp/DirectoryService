using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Positions.Failures;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace DirectoryService.Application.Positions.Commands.SoftDelete;

public class SoftDeletePositionHandler(
    ILogger<SoftDeletePositionValidator> logger,
    IValidator<SoftDeletePositionCommand> validator,
    IPositionsRepository positionsRepository,
    ITransactionManager transactionManager)
    : ICommandHandler<SoftDeletePositionCommand>
{
    public async Task<UnitResult<Errors>> Handle(SoftDeletePositionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var positionId = new PositionId(command.PositionId);

        var getPositionResult = await positionsRepository.GetByIdWithLock(positionId, cancellationToken);
        if (getPositionResult.IsFailure)
        {
            return getPositionResult.Error.ToErrors();
        }

        var position = getPositionResult.Value;

        if (!position.IsActive)
        {
            return PositionsErrors.Inactive(positionId.Value).ToErrors();
        }

        position.Deactivate();

        var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error.ToErrors();
        }

        logger.LogInformation("Success soft delete position with id [{positionId}]", positionId.Value);

        return UnitResult.Success<Errors>();
    }
}