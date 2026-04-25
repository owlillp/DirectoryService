using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentHandler(
    ILogger<CreateDepartmentHandler> logger,
    IValidator<CreateDepartmentCommand> validator,
    IDepartmentsRepository departmentsRepository,
    ILocationsRepository locationsRepository)
    : ICommandHandler<Guid, CreateDepartmentCommand>
{
    public async Task<Result<Guid, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var request = command.Request;

        var departmentId = new DepartmentId(Guid.NewGuid());

        var departmentName = DepartmentName.Create(request.Name).Value;

        var departmentIdentifier = DepartmentIdentifier.Create(request.Identifier).Value;

        var locationIds = request.LocationIds
            .Select(l => new LocationId(l))
            .ToArray();

        var locationValidationResult = await locationsRepository.ExistAsync(locationIds, cancellationToken);
        if (locationValidationResult.IsFailure)
        {
            return locationValidationResult.Error.ToErrors();
        }

        if (!locationValidationResult.Value)
        {
            return GeneralErrors.NotFound(nameof(Location)).ToErrors();
        }

        var departmentLocations = locationIds
            .Select(l => new DepartmentLocation(departmentId, l));

        var departmentResult = request.ParentId.HasValue
            ? await CreateChildDepartment()
            : Department.CreateParent(departmentName, departmentIdentifier, departmentLocations);

        if (departmentResult.IsFailure)
        {
            return departmentResult.Error.ToErrors();
        }

        var addResult = await departmentsRepository.AddAsync(departmentResult.Value, cancellationToken);
        if (addResult.IsFailure)
        {
            return addResult.Error.ToErrors();
        }

        logger.LogInformation("Success created department with id: {departmentId}", addResult.Value.Value);

        return addResult.Value.Value;

        async Task<Result<Department, Error>> CreateChildDepartment()
        {
            var parentId = new DepartmentId(request.ParentId.Value);

            var getParentResult = await departmentsRepository
                .GetByAsync(d => d.Id == parentId, cancellationToken);
            if (getParentResult.IsFailure)
            {
                return getParentResult.Error;
            }

            return Department.CreateChild(
                departmentName,
                departmentIdentifier,
                getParentResult.Value,
                departmentLocations,
                departmentId);
        }
    }
}