using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Requests;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Queries.SearchDepartments;

public class SearchDepartmentsValidator : AbstractValidator<SearchDepartmentsQuery>
{
    public SearchDepartmentsValidator()
    {
        RuleFor(q => q.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(SearchDepartmentsQuery.Request)));

        When(q => q.Request != null!, () =>
        {
            RuleFor(q => q.Request.Name)
                .NotNull().WithError(GeneralErrors.ValueIsRequired(nameof(SearchDepartmentsQuery.Request)))
                .NotEmpty().WithError(GeneralErrors.ValueIsRequired(nameof(SearchDepartmentsQuery.Request)));

            When(q => !string.IsNullOrWhiteSpace(q.Request.Name), () =>
            {
                RuleFor(q => q.Request.Name)
                    .Must(name => name.Length >= 2)
                    .WithError(GeneralErrors.InvalidLength(
                        nameof(SearchDepartmentRequest),
                        nameof(SearchDepartmentRequest.Name)));
            });

            RuleFor(q => q.Request.Page)
                .GreaterThan(0)
                .WithError(GeneralErrors.ValueIsInvalid(nameof(SearchDepartmentRequest.Page), "Value must be greater than zero."));

            RuleFor(q => q.Request.PageSize)
                .GreaterThan(0)
                .WithError(GeneralErrors.ValueIsInvalid(nameof(SearchDepartmentRequest.PageSize), "Value must be greater than zero."));
        });
    }
}