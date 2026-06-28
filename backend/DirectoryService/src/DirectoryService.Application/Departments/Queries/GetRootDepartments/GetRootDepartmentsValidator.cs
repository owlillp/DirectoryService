using Core.Validation;
using DirectoryService.Contracts.Departments.Requests;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Departments.Queries.GetRootDepartments;

public class GetRootDepartmentsValidator : AbstractValidator<GetRootDepartmentsQuery>
{
    public GetRootDepartmentsValidator()
    {
        RuleFor(q => q.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetRootDepartmentsQuery.Request)));

        RuleFor(q => q.Request.Page)
            .GreaterThan(0)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(GetRootDepartmentsRequest.Page), "Value must be greater than zero."));

        RuleFor(q => q.Request.Size)
            .GreaterThan(0)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(GetRootDepartmentsRequest.Size), "Value must be greater than zero."));

        RuleFor(q => q.Request.Prefetch)
            .GreaterThanOrEqualTo(0)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(GetRootDepartmentsRequest.Prefetch), "Value must be greater or equal to zero."));
    }
}