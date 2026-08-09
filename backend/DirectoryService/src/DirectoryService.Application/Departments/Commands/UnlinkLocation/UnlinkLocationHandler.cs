using Core.Abstractions;
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

namespace DirectoryService.Application.Departments.Commands.UnlinkLocation;

public class UnlinkLocationHandler(
    ILogger<UnlinkLocationHandler> logger,
    IValidator<UnlinkLocationCommand> validator,
    IDepartmentsRepository departmentsRepository,
    IReadDbContext readDbContext,
    LocationCacheInvalidator cacheInvalidator)
    : ICommandHandler<UnlinkLocationCommand>
{
    public async Task<UnitResult<Errors>> Handle(UnlinkLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var departmentId = new DepartmentId(command.DepartmentId);
        var locationId = new LocationId(command.LocationId);

        bool existReference = await readDbContext
            .DepartmentLocationsRead
            .AnyAsync(
                dl => dl.DepartmentId == departmentId && dl.LocationId == locationId,
                cancellationToken);

        if (!existReference)
        {
            return GeneralErrors
                .NotFound(nameof(UnlinkLocationCommand.LocationId), locationId.Value)
                .ToErrors();
        }

        var unlinkResult = await departmentsRepository.UnlinkLocationAsync(departmentId, locationId, cancellationToken);
        if (unlinkResult.IsFailure)
        {
            return unlinkResult.Error.ToErrors();
        }

        await cacheInvalidator.InvalidateDepartmentListsAsync(departmentId.Value, cancellationToken);

        logger.LogInformation(
            "Success unlink location with id [{locationId}] to department with id [{departmentId}]",
            locationId.Value,
            departmentId.Value);

        return UnitResult.Success<Errors>();
    }
}