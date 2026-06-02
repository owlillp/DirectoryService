using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Positions.Commands.SoftDelete;

public class SoftDeletePositionValidator : AbstractValidator<SoftDeletePositionCommand>
{
    public SoftDeletePositionValidator()
    {
        RuleFor(c => c.PositionId)
            .Must(i => i != Guid.Empty)
            .WithError(GeneralErrors.ValueIsRequired(nameof(SoftDeletePositionCommand.PositionId)));
    }
}