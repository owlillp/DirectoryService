using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Departments.Commands.LinkLocation;

public class LinkLocationHandler(
    ILogger<LinkLocationHandler> logger,
    IValidator<LinkLocationCommand> validator,
    IDepartmentsRepository departmentsRepository,
    IReadDbContext readDbContext,
    ITransactionManager transactionManager,
    LocationCacheInvalidator cacheInvalidator)
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

        bool locationExist = await readDbContext.LocationsRead.AnyAsync(
            l => l.Id == locationId, cancellationToken: cancellationToken);
        if (!locationExist)
        {
            return GeneralErrors.NotFound(nameof(Location), locationId.Value).ToErrors();
        }

        bool alreadyLinked = await readDbContext.DepartmentLocationsRead.AnyAsync(
            dl => dl.DepartmentId == departmentId && dl.LocationId == locationId, cancellationToken);

        if (alreadyLinked)
        {
            return GeneralErrors
                .Conflict(locationId.Value.ToString(), nameof(LinkLocationCommand.LocationId))
                .ToErrors();
        }

        var getResult = await departmentsRepository.GetByIdWithLockAsync(departmentId, cancellationToken);
        if (getResult.IsFailure)
        {
            return getResult.Error.ToErrors();
        }

        var department = getResult.Value;

        department.LinkLocation(locationId);

        var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error.ToErrors();
        }

        await cacheInvalidator.InvalidateDepartmentListsAsync(departmentId.Value, cancellationToken);

        logger.LogInformation(
            "Success link location with id [{locationId}] to department with id [{departmentId}]",
            locationId.Value,
            departmentId.Value);

        return UnitResult.Success<Errors>();
    }
}