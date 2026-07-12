using Core.Validation;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Positions.Requests;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Positions.Queries.GetCursorPositions;

public class GetCursorPositionsValidator : AbstractValidator<GetCursorPositionsQuery>
{
    public GetCursorPositionsValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetCursorPositionsQuery.Request)));

        When(x => x.Request != null!, () =>
        {
            RuleFor(q => q.Request.Search)
                .MaximumLength(1000)
                .WithError(GeneralErrors.InvalidLength(nameof(GetCursorPositionsQuery.Request.Search)));

            RuleFor(x => x.Request.CursorRequest)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired(nameof(GetCursorPositionsRequest.CursorRequest)));

            When(x => x.Request.CursorRequest != null!, () =>
            {
                When(x => x.Request.CursorRequest.Cursor != null, () =>
                {
                    RuleFor(x => x.Request.CursorRequest.Cursor!.Id)
                        .NotEmpty()
                        .WithError(GeneralErrors.ValueIsRequired(nameof(CursorPaginationRequest.Cursor)));

                    When(x => x.Request.SortBy is "created_at", () =>
                    {
                        RuleFor(x => x.Request.CursorRequest.Cursor!.Value)
                            .Must(x => DateTime.TryParse(x, out _))
                            .WithError(GeneralErrors.ValueIsInvalid(nameof(CursorPaginationRequest.Cursor.Value)));
                    });
                });

                RuleFor(x => x.Request.CursorRequest.Limit)
                    .GreaterThan(0)
                    .WithError(GeneralErrors.NegativeValue(nameof(CursorPaginationRequest.Limit)));
            });
        });
    }
}