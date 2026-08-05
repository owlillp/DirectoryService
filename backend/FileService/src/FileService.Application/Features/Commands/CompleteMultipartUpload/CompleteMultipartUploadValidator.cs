using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Commands.CompleteMultipartUpload;

public class CompleteMultipartUploadValidator : AbstractValidator<CompleteMultipartUploadCommand>
{
    public CompleteMultipartUploadValidator()
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

            RuleFor(f => f.Request.UploadId)
                .NotNull()
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired("uploadId"));

            RuleForEach(c => c.Request.PartETags)
                .NotNull()
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired("partETags"));
        });
    }
}