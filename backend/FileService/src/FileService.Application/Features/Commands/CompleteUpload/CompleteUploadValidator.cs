using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Commands.CompleteUpload;

public class CompleteUploadValidator : AbstractValidator<CompleteUploadCommand>
{
    public CompleteUploadValidator()
    {
        RuleFor(c => c.FileId)
            .NotNull()
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("fileId"));
    }
}