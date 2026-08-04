using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Queries.GetMediaAsset;

public class GetMediaAssetValidator : AbstractValidator<GetMediaAssetQuery>
{
    public GetMediaAssetValidator()
    {
        RuleFor(q => q.FileId)
            .NotNull()
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("fileId"));
    }
}