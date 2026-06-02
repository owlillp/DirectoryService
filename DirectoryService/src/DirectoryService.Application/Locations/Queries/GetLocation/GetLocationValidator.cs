using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Locations.Queries.GetLocation;

public class GetLocationValidator : AbstractValidator<GetLocationQuery>
{
    public GetLocationValidator()
    {
        RuleFor(q => q.LocationId)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetLocationQuery.LocationId)));
    }
}