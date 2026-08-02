using Core.Validation;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Queries.GetFile;

public class GetFileValidator : AbstractValidator<GetFileQuery>
{
    public GetFileValidator()
    {
        RuleFor(c => c.FileId)
            .NotNull()
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("fileId"));
    }
}