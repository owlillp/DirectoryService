using DirectoryService.Application.Validation;
using FluentValidation;
using Shared.Failures;

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