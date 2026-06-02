using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Departments.Queries.GetTopDepartmentsByPositions;

public class GetTopDepartmentsByPositionsValidator : AbstractValidator<GetTopDepartmentsByPositionQuery>
{
    public GetTopDepartmentsByPositionsValidator()
    {
        RuleFor(q => q.TopCount)
            .Must(c => c > 0)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(GetTopDepartmentsByPositionQuery.TopCount), "Value must be greater than zero."));

        RuleFor(q => q.TopCount)
            .Must(c => c <= 1000)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(GetTopDepartmentsByPositionQuery.TopCount), "Value is greatest"));
    }
}