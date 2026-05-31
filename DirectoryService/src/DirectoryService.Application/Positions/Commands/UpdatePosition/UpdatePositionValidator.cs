using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Positions.Commands.UpdatePosition;

public class UpdatePositionValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionValidator()
    {
        RuleFor(c => c.PositionId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdatePositionCommand.PositionId)));

        RuleFor(c => c.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdatePositionCommand.Request)));

        When(c => c.Request != null!, () =>
        {
            RuleFor(c => c.Request.Name)
                .MustBeValueObject(PositionName.Create);
        });
    }
}