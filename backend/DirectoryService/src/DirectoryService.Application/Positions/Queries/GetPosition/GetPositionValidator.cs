using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

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