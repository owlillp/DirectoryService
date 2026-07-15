using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Departments.Queries.SearchDepartments;

public class SearchDepartmentsValidator : AbstractValidator<SearchDepartmentsQuery>
{
    public SearchDepartmentsValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(SearchDepartmentsQuery.Request)));

        When(x => x.Request != null!, () =>
        {
            RuleFor(x => x.Request.ParentId)
                .Must(x => !x.HasValue || x.Value != Guid.Empty)
                .WithError(GeneralErrors.ValueIsInvalid("searchRequest", "empty guid", "parentId"));

            RuleFor(x => x.Request.Page)
                .GreaterThan(0)
                .WithError(GeneralErrors.ValueIsInvalid("searchRequest", "value must be greater than zero", "page"));

            RuleFor(x => x.Request.PageSize)
                .LessThanOrEqualTo(50)
                .WithError(GeneralErrors.ValueIsInvalid("searchRequest", "value must be less or equal than 50",
                    "pageSize"));
        });
    }
}