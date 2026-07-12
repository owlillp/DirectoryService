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
            RuleFor(x => x.Request.CursorRequest)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired(nameof(GetCursorPositionsRequest.CursorRequest)));

            When(x => x.Request.CursorRequest != null!, () =>
            {
                When(x => x.Request.CursorRequest.Cursor.HasValue, () =>
                {
                    RuleFor(x => x.Request.CursorRequest.Cursor)
                        .NotEmpty()
                        .WithError(GeneralErrors.ValueIsRequired(nameof(CursorPaginationRequest.Cursor)));
                });

                RuleFor(x => x.Request.CursorRequest.Limit)
                    .GreaterThan(0)
                    .WithError(GeneralErrors.NegativeValue(nameof(CursorPaginationRequest.Limit)));
            });
        });
    }
}