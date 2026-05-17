using DirectoryService.Application.Validation;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Commands.SoftDeleteDepartment;

public class SoftDeleteDepartmentValidator : AbstractValidator<SoftDeleteDepartmentCommand>
{
    public SoftDeleteDepartmentValidator()
    {
        RuleFor(c => c.DepartmentId)
            .Must(i => i != Guid.Empty)
            .WithError(GeneralErrors.ValueIsRequired(nameof(SoftDeleteDepartmentCommand.DepartmentId)));
    }
}