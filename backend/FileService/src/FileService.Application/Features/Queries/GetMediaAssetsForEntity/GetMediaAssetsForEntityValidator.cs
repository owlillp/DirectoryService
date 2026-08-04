using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Queries.GetMediaAssetsForEntity;

public class GetMediaAssetsForEntityValidator : AbstractValidator<GetMediaAssetsForEntityQuery>
{
    public GetMediaAssetsForEntityValidator()
    {
        RuleFor(q => q.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("request"));

        When(q => q.Request != null!, () =>
        {
            RuleFor(q => q.Request.Context)
                .NotNull()
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired("context"));

            RuleFor(q => q.Request.EntityId)
                .NotNull()
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired("entityId"));
        });
    }
}