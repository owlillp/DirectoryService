using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Commands.Delete;

public class DeleteMediaAssetValidator : AbstractValidator<DeleteMediaAssetCommand>
{
    public DeleteMediaAssetValidator()
    {
        RuleFor(q => q.FileId)
            .NotNull()
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("fileId"));
    }
}