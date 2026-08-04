using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Commands.AbortUpload;

public class AbortUploadValidator : AbstractValidator<AbortUploadCommand>
{
    public AbortUploadValidator()
    {
        RuleFor(q => q.FileId)
            .NotNull()
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("fileId"));
    }
}