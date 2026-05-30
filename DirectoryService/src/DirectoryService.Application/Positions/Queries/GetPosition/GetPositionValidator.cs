using DirectoryService.Application.Validation;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Positions.Queries.GetPosition;

public class GetPositionValidator : AbstractValidator<GetPositionQuery>
{
    public GetPositionValidator()
    {
        RuleFor(x => x.PositionId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetPositionQuery.PositionId)));
    }
}