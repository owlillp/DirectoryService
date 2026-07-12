using Core.Validation;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Positions.Requests;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Positions.Queries.GetPositionsInfinite;

public class GetPositionsInfiniteValidator : AbstractValidator<GetPositionsInfiniteQuery>
{
    public GetPositionsInfiniteValidator()
    {
        RuleFor(x => x.InfiniteRequest)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetPositionsInfiniteQuery.InfiniteRequest)));

        When(x => x.InfiniteRequest != null!, () =>
        {
            RuleFor(q => q.InfiniteRequest.Search)
                .MaximumLength(1000)
                .WithError(GeneralErrors.InvalidLength(nameof(GetPositionsInfiniteQuery.InfiniteRequest.Search)));

            RuleFor(x => x.InfiniteRequest.InfiniteRequest)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired(nameof(GetPositionsInfiniteRequest.InfiniteRequest)));

            When(x => x.InfiniteRequest.InfiniteRequest != null!, () =>
            {
                When(x => x.InfiniteRequest.InfiniteRequest.Cursor != null, () =>
                {
                    RuleFor(x => x.InfiniteRequest.InfiniteRequest.Cursor!.Id)
                        .NotEmpty()
                        .WithError(GeneralErrors.ValueIsRequired(nameof(InfinitePaginationRequest.Cursor)));

                    When(x => x.InfiniteRequest.SortBy is "created_at", () =>
                    {
                        RuleFor(x => x.InfiniteRequest.InfiniteRequest.Cursor!.Value)
                            .Must(x => DateTime.TryParse(x, out _))
                            .WithError(GeneralErrors.ValueIsInvalid(nameof(InfinitePaginationRequest.Cursor.Value)));
                    });
                });

                RuleFor(x => x.InfiniteRequest.InfiniteRequest.Limit)
                    .GreaterThan(0)
                    .WithError(GeneralErrors.NegativeValue(nameof(InfinitePaginationRequest.Limit)));
            });
        });
    }
}