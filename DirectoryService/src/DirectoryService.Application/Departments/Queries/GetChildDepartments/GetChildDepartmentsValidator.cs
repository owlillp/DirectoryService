using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Requests;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Queries.GetChildDepartments;

public class GetChildDepartmentsValidator : AbstractValidator<GetChildDepartmentsQuery>
{
    public GetChildDepartmentsValidator()
    {
        RuleFor(q => q.ParentId)
            .NotNull()
            .Must(p => p != Guid.Empty)
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetChildDepartmentsQuery.ParentId)));

        RuleFor(q => q.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetChildDepartmentsQuery.Request)));

        RuleFor(q => q.Request.Page)
            .GreaterThan(0)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(GetChildDepartmentsRequest.Page), "Value must be greater than zero."));

        RuleFor(q => q.Request.Size)
            .GreaterThan(0)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(GetChildDepartmentsRequest.Size), "Value must be greater than zero."));
    }
}