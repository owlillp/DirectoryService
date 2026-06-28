using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Departments.Commands.UnlinkPosition;

public class UnlinkPositionValidator : AbstractValidator<UnlinkPositionCommand>
{
    public UnlinkPositionValidator()
    {
        RuleFor(c => c.DepartmentId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(UnlinkPositionCommand.DepartmentId)));

        RuleFor(c => c.PositionId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(UnlinkPositionCommand.PositionId)));
    }
}