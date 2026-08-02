using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Domain;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Queries.GetFile;

public class GetFileHandler(
    IValidator<GetFileQuery> validator,
    IFileStorageProvider fileStorageProvider,
    IMediaAssetRepository repository) : IQueryHandler<string, GetFileQuery>
{
    public async Task<Result<string, Errors>> Handle(GetFileQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var getMediaAssetResult = await repository.GetByAsync(ma => ma.Id == query.FileId, cancellationToken);
        if (getMediaAssetResult.IsFailure)
        {
            return getMediaAssetResult.Error.ToErrors();
        }

        var mediaAsset = getMediaAssetResult.Value;
        if (mediaAsset.Status != MediaStatus.UPLOADED)
        {
            return FileErrors.ObjectNotFound().ToErrors();
        }

        var generateUrlResult = await fileStorageProvider.GenerateDownloadUrlAsync(mediaAsset.Key);
        if (generateUrlResult.IsFailure)
        {
            return generateUrlResult.Error.ToErrors();
        }

        return generateUrlResult.Value;
    }
}