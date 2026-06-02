using Core.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(c => c.DepartmentId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdateDepartmentCommand.DepartmentId)));

        RuleFor(c => c.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdateDepartmentCommand.Request)));

        When(c => c.Request != null!, () =>
        {
            RuleFor(c => c.Request.Name)
                .MustBeValueObject(DepartmentName.Create);
        });
    }
}