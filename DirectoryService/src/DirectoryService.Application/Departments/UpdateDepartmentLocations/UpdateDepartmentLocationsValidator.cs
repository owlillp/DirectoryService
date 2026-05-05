using DirectoryService.Application.Validation;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Departments.UpdateDepartmentLocations;

public class UpdateDepartmentLocationsValidator : AbstractValidator<UpdateDepartmentLocationsCommand>
{
    public UpdateDepartmentLocationsValidator()
    {
        RuleFor(d => d.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdateDepartmentLocationsCommand.Request)));

        RuleFor(d => d.DepartmentId)
            .NotNull().WithError(GeneralErrors.ValueIsRequired(nameof(UpdateDepartmentLocationsCommand.DepartmentId)))
            .NotEmpty().WithError(GeneralErrors.ValueIsInvalid(nameof(UpdateDepartmentLocationsCommand.DepartmentId)));

        RuleForEach(c => c.Request.LocationIds)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdateDepartmentLocationsCommand.Request.LocationIds)));

        RuleFor(p => p.Request.LocationIds)
            .Must(locationIds => locationIds.Any())
            .WithError(GeneralErrors.InvalidLength(nameof(UpdateDepartmentLocationsCommand.Request.LocationIds)));

        RuleFor(p => p.Request.LocationIds)
            .Must(locationIds =>
            {
                var enumerable = locationIds.ToArray();
                return enumerable.Distinct().Count() == enumerable.Length;
            })
            .WithError(GeneralErrors.Duplicate(nameof(UpdateDepartmentLocationsCommand.Request.LocationIds)));
    }
}