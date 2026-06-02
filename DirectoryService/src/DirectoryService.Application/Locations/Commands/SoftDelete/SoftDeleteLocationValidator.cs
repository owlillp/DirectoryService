using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Locations.Commands.SoftDelete;

public class SoftDeleteLocationValidator : AbstractValidator<SoftDeleteLocationCommand>
{
    public SoftDeleteLocationValidator()
    {
        RuleFor(c => c.LocationId)
            .Must(i => i != Guid.Empty)
            .WithError(GeneralErrors.ValueIsRequired(nameof(SoftDeleteLocationCommand.LocationId)));
    }
}