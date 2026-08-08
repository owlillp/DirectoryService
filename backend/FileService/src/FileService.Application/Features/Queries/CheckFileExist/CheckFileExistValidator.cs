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

        When(x => !string.IsNullOrWhiteSpace(x.MediaType), () =>
        {
            RuleFor(x => x.MediaType)
                .Must(at => Enum.TryParse<MediaType>(at, ignoreCase: true, out _))
                .WithError(GeneralErrors.ValueIsInvalid("mediaType"));
        });
    }
}