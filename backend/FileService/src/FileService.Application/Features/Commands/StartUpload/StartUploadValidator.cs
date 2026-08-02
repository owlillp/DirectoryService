using Core.Validation;
using FileService.Application.Features.Commands.StartUpload;
using FileService.Domain;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Commands.GenerateUploadUrl;

public class StartUploadValidator : AbstractValidator<StartUploadCommand>
{
    public StartUploadValidator()
    {
        RuleFor(command => command.Request)
            .NotNull()
            .NotEmpty();

        When(command => command.Request != null!, () =>
        {
            RuleFor(command => command.Request.FileName)
                .MustBeValueObject(FileName.Create);

            RuleFor(command => command.Request.ContentType)
                .MustBeValueObject(ContentType.Create);

            RuleFor(command => command.Request)
                .MustBeValueObject((r) => MediaOwner.Create(r.Context, r.ContextId));

            RuleFor(command => command.Request.Size)
                .GreaterThan(0)
                .WithError(GeneralErrors.ValueIsInvalid("size"));

            RuleFor(command => command.Request.AssetType)
                .Must(c => c.IsAssetType())
                .WithError(GeneralErrors.ValueIsInvalid("assetType"));
        });
    }
}