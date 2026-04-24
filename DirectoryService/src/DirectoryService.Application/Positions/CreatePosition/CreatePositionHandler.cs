using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionHandler(
    ILogger<CreatePositionHandler> logger,
    IValidator<CreatePositionCommand> validator,
    IPositionsRepository positionsRepository,
    IDepartmentsRepository departmentsRepository)
    : ICommandHandler<Guid, CreatePositionCommand>
{
    public async Task<Result<Guid, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var request = command.Request;

        var positionId = new PositionId(Guid.NewGuid());

        var positionName = PositionName.Create(request.Name).Value;

        var nameValidationResult = await positionsRepository.IsNameUniqueAsync(positionName, cancellationToken);
        if (nameValidationResult.IsFailure)
        {
            return nameValidationResult.Error.ToErrors();
        }

        if (!nameValidationResult.Value)
        {
            return GeneralErrors.Conflict(nameof(Position), nameof(Position.Name)).ToErrors();
        }

        var positionDescription = request.Description == null
            ? null
            : PositionDescription.Create(request.Description).Value;

        var departmentIds = request.DepartmentIds
            .Select(d => new DepartmentId(d))
            .ToArray();

        var departmentsValidationResult = await departmentsRepository.ExistAndActiveAsync(departmentIds, cancellationToken);
        if (departmentsValidationResult.IsFailure)
        {
            return departmentsValidationResult.Error.ToErrors();
        }

        if (!departmentsValidationResult.Value)
        {
            return GeneralErrors.NotFound(nameof(Department)).ToErrors();
        }

        var departmentPositions = departmentIds
            .Select(d => new DepartmentPosition(d, positionId));

        var position = Position.Create(
            positionName,
            positionDescription,
            departmentPositions);

        var addResult = await positionsRepository.AddAsync(position, cancellationToken);
        if (addResult.IsFailure)
        {
            return addResult.Error.ToErrors();
        }

        logger.LogInformation("Success created position with id: {positionId}", addResult.Value.Value);

        return addResult.Value.Value;
    }
}