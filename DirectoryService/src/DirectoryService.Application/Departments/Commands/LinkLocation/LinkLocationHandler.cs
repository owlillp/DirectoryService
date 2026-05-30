using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Commands.LinkLocation;

public class LinkLocationHandler(
    ILogger<LinkLocationHandler> logger,
    IValidator<LinkLocationCommand> validator,
    IDepartmentsRepository departmentsRepository,
    ITransactionManager transactionManager)
    : ICommandHandler<LinkLocationCommand>
{
    public async Task<UnitResult<Errors>> Handle(LinkLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var departmentId = new DepartmentId(command.DepartmentId);
        var locationId = new LocationId(command.LocationId);

        var getResult = await departmentsRepository.GetByIdWithLockAsync(departmentId, cancellationToken, includeLocations: true);
        if (getResult.IsFailure)
        {
            return getResult.Error.ToErrors();
        }

        var department = getResult.Value;

        if (department.Locations.Any(dl => dl.LocationId == locationId))
        {
            return GeneralErrors
                .Conflict(locationId.Value.ToString(), nameof(LinkLocationCommand.LocationId))
                .ToErrors();
        }

        department.LinkLocation(locationId);

        var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error.ToErrors();
        }

        logger.LogInformation(
            "Success link location with id [{locationId}] to department with id [{departmentId}]",
            locationId.Value,
            departmentId.Value);

        return UnitResult.Success<Errors>();
    }
}