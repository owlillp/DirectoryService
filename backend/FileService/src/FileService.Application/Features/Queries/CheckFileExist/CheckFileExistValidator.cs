using Core.Validation;
using FileService.Domain;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Queries.CheckFileExist;

public class CheckFileExistValidator : AbstractValidator<CheckFileExistQuery>
{
    public CheckFileExistValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty()
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("fileId"));

        When(x => !string.IsNullOrWhiteSpace(x.AssetType), () =>
        {
            RuleFor(x => x.AssetType)
                .Must(at => Enum.IsDefined(typeof(AssetType), at!))
                .WithError(GeneralErrors.ValueIsInvalid("assetType"));
        });
    }
}