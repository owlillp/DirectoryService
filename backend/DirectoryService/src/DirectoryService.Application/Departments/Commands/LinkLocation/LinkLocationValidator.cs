using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Departments.Commands.LinkLocation;

public class LinkLocationValidator : AbstractValidator<LinkLocationCommand>
{
    public LinkLocationValidator()
    {
        RuleFor(c => c.DepartmentId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(LinkLocationCommand.DepartmentId)));

        RuleFor(c => c.LocationId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(LinkLocationCommand.LocationId)));
    }
}