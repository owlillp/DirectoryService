using Core.Validation;
using DirectoryService.Contracts.Departments.Requests;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Departments.Queries.SearchDepartmentAncestors;

public class SearchDepartmentsValidator : AbstractValidator<SearchDepartmentAncestorsQuery>
{
    public SearchDepartmentsValidator()
    {
        RuleFor(q => q.AncestorsRequest)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(SearchDepartmentAncestorsQuery.AncestorsRequest)));

        When(q => q.AncestorsRequest != null!, () =>
        {
            RuleFor(q => q.AncestorsRequest.Name)
                .NotNull().WithError(GeneralErrors.ValueIsRequired(nameof(SearchDepartmentAncestorsQuery.AncestorsRequest)))
                .NotEmpty().WithError(GeneralErrors.ValueIsRequired(nameof(SearchDepartmentAncestorsQuery.AncestorsRequest)));

            When(q => !string.IsNullOrWhiteSpace(q.AncestorsRequest.Name), () =>
            {
                RuleFor(q => q.AncestorsRequest.Name)
                    .Must(name => name.Length >= 2)
                    .WithError(GeneralErrors.InvalidLength(
                        nameof(SearchDepartmentAncestorsRequest),
                        nameof(SearchDepartmentAncestorsRequest.Name)));
            });

            RuleFor(q => q.AncestorsRequest.Page)
                .GreaterThan(0)
                .WithError(GeneralErrors.ValueIsInvalid(nameof(SearchDepartmentAncestorsRequest.Page), "Value must be greater than zero."));

            RuleFor(q => q.AncestorsRequest.PageSize)
                .GreaterThan(0)
                .WithError(GeneralErrors.ValueIsInvalid(nameof(SearchDepartmentAncestorsRequest.PageSize), "Value must be greater than zero."));
        });
    }
}