using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Commands.AbortMultipartUpload;

public class AbortMultipartUploadValidator : AbstractValidator<AbortMultipartUploadCommand>
{
    public AbortMultipartUploadValidator()
    {
        RuleFor(c => c.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsInvalid("request"));

        When(c => c.Request != null!, () =>
        {
            RuleFor(c => c.Request.FileId)
                .NotNull()
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired("fileId"));

            RuleFor(c => c.Request.UploadId)
                .NotNull()
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired("uploadId"));
        });
    }
}