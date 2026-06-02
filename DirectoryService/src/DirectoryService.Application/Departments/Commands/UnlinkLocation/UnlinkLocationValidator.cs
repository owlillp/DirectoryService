using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Departments.Commands.UnlinkLocation;

public class UnlinkLocationValidator : AbstractValidator<UnlinkLocationCommand>
{
    public UnlinkLocationValidator()
    {
        RuleFor(c => c.DepartmentId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(UnlinkLocationCommand.DepartmentId)));

        RuleFor(c => c.LocationId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(UnlinkLocationCommand.LocationId)));
    }
}