using DirectoryService.Application.Validation;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Queries.GetDepartment;

public class GetDepartmentValidator : AbstractValidator<GetDepartmentQuery>
{
    public GetDepartmentValidator()
    {
        RuleFor(x => x.DepartmentId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetDepartmentQuery.DepartmentId)));
    }
}