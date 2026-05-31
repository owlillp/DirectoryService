using DirectoryService.Application.Validation;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Locations.Queries.GetTopLocationsByPositions;

public class GetTopLocationsValidator : AbstractValidator<GetTopLocationsQuery>
{
    public GetTopLocationsValidator()
    {
        RuleFor(query => query.TopCount)
            .GreaterThan(0)
            .WithError(GeneralErrors.InvalidLength(nameof(GetTopLocationsQuery.TopCount), "Value must be greater than zero."));

        RuleFor(q => q.TopCount)
            .Must(c => c <= 1000)
            .WithError(GeneralErrors.ValueIsInvalid(nameof(GetTopLocationsQuery.TopCount), "Value is greatest"));
    }
}