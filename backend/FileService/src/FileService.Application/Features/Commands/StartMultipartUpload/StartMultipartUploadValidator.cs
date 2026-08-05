using Core.Validation;
using FileService.Domain;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Commands.StartMultipartUpload;

public class StartMultipartUploadValidator : AbstractValidator<StartMultipartUploadCommand>
{
    public StartMultipartUploadValidator()
    {
        RuleFor(c => c.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsInvalid("request"));

        When(c => c.Request != null!, () =>
        {
            RuleFor(c => c.Request)
                .MustBeValueObject(r => MediaOwner.Create(r.Context, r.ContextId));

            RuleFor(c => c.Request.ContentType)
                .MustBeValueObject(ContentType.Create);

            RuleFor(c => c.Request.FileName)
                .NotNull()
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired("fileName"));

            RuleFor(c => c.Request.Size)
                .GreaterThan(0)
                .WithError(GeneralErrors.ValueIsRequired("size"));

            RuleFor(c => c.Request.AssetType)
                .Must(type => type.IsAssetType())
                .WithError(GeneralErrors.ValueIsInvalid("assetType"));
        });
    }
}