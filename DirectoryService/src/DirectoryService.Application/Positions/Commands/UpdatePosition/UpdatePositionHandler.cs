using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Positions.Commands.UpdatePosition;

public class UpdatePositionHandler(
    ILogger<UpdatePositionHandler> logger,
    IValidator<UpdatePositionCommand> validator,
    ITransactionManager transactionManager,
    IPositionsRepository positionRepository) : ICommandHandler<UpdatePositionCommand>
{
    public async Task<UnitResult<Errors>> Handle(UpdatePositionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var positionId = new PositionId(command.PositionId);
        var positionName = PositionName.Create(command.Request.Name).Value;

        var getResult = await positionRepository.GetByAsync(p => p.Id == positionId && p.IsActive, cancellationToken);
        if (getResult.IsFailure)
        {
            return getResult.Error.ToErrors();
        }

        var position = getResult.Value;
        var destinationName = position.Name;

        position.Rename(positionName);

        var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error.ToErrors();
        }

        logger.LogInformation(
            "Success update position with id [{PositionId}] : {destinationPositionName} => {NewPositionName}",
            positionId,
            destinationName.Value,
            positionName.Value);

        return UnitResult.Success<Errors>();
    }
}