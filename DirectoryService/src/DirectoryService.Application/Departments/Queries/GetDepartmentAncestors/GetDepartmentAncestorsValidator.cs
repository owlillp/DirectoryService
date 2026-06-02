using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentAncestors;

public class GetDepartmentAncestorsValidator : AbstractValidator<GetDepartmentAncestorsQuery>
{
    public GetDepartmentAncestorsValidator()
    {
        RuleFor(q => q.TargetDepartmentId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(GetDepartmentAncestorsQuery.TargetDepartmentId)));
    }
}