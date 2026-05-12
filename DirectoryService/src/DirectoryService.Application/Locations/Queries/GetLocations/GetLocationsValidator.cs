using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Common;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsValidator : AbstractValidator<GetLocationsQuery>
{
    public GetLocationsValidator()
    {
        RuleFor(q => q.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetLocationsQuery.Request)));

        RuleFor(q => q.Request.Search)
            .MaximumLength(1000)
            .WithError(GeneralErrors.InvalidLength(nameof(GetLocationsQuery.Request.Search)));

        RuleFor(q => q.Request.Pagination)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetLocationsQuery.Request.Pagination)));

        RuleFor(q => q.Request.Pagination!.Page)
            .GreaterThan(0)
            .When(q => q.Request.Pagination != null)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(PaginationRequest), "Must be greater than 0", nameof(PaginationRequest.Page)));

        RuleFor(q => q.Request.Pagination!.PageSize)
            .GreaterThan(0)
            .When(q => q.Request.Pagination != null)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(PaginationRequest), "Must be greater than 0", nameof(PaginationRequest.PageSize)));

        RuleFor(q => q.Request.DepartmentIds)
            .Must(ids => ids == null || ids.Distinct().Count() == ids.Length)
            .WithError(GeneralErrors.Duplicate(nameof(GetLocationsQuery.Request.DepartmentIds)));
    }
}