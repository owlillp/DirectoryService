using DirectoryService.Application.Validation;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Commands.LinkPosition;

public class LinkPositionValidator : AbstractValidator<LinkPositionCommand>
{
    public LinkPositionValidator()
    {
        RuleFor(c => c.DepartmentId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(LinkPositionCommand.DepartmentId)));

        RuleFor(c => c.PositionId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(LinkPositionCommand.PositionId)));
    }
}