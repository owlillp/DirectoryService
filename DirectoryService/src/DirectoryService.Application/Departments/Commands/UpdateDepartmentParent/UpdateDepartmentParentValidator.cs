using DirectoryService.Application.Departments.Failures;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.Requests;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartmentParent;

public class UpdateDepartmentParentValidator : AbstractValidator<UpdateDepartmentParentCommand>
{
    public UpdateDepartmentParentValidator()
    {
        RuleFor(c => c.DepartmentId)
            .NotNull().WithError(GeneralErrors.ValueIsRequired(nameof(UpdateDepartmentParentCommand.DepartmentId)))
            .NotEmpty().WithError(GeneralErrors.ValueIsInvalid(nameof(UpdateDepartmentParentCommand.DepartmentId)));

        RuleFor(c => c.Request.ParentId)
            .Must(pId => pId != Guid.Empty)
            .When(c => c.Request.ParentId.HasValue)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(UpdateDepartmentParentRequest.ParentId)));

        RuleFor(c => c)
            .Must(c => c.DepartmentId != c.Request.ParentId)
            .When(c => c.Request.ParentId.HasValue)
            .WithError(DepartmentsErrors.SelfRepeat());
    }
}