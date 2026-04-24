using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
        RuleFor(c => c.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(CreateDepartmentCommand.Request)));

        RuleFor(d => d.Request.Name)
            .MustBeValueObject(DepartmentName.Create);

        RuleFor(d => d.Request.Identifier)
            .MustBeValueObject(DepartmentIdentifier.Create);

        RuleForEach(c => c.Request.LocationIds)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(CreateDepartmentCommand.Request.LocationIds)));

        RuleFor(p => p.Request.LocationIds)
            .Must(locationIds => locationIds.Any())
            .WithError(GeneralErrors.InvalidLength(nameof(CreateDepartmentCommand.Request.LocationIds)));

        RuleFor(p => p.Request.LocationIds)
            .Must(locationIds =>
            {
                var enumerable = locationIds.ToArray();
                return enumerable.Distinct().Count() == enumerable.Length;
            })
            .WithError(GeneralErrors.Duplicate(nameof(CreateDepartmentCommand.Request.LocationIds)));
    }
}