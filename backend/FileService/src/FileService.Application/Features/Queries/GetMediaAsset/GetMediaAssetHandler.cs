using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Contracts.Files.Dtos;
using FileService.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Queries.GetMediaAsset;

public class GetMediaAssetHandler(
    IValidator<GetMediaAssetQuery> validator,
    IFileStorageProvider fileStorageProvider,
    IReadDbContext readDbContext) : IQueryHandler<GetMediaAssetDto, GetMediaAssetQuery>
{
    public async Task<Result<GetMediaAssetDto, Errors>> Handle(GetMediaAssetQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var mediaAsset = await readDbContext.MediaAssetsRead
            .Where(ma => ma.Id == query.FileId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (mediaAsset == null)
        {
            return FileErrors.ObjectNotFound(query.FileId.ToString()).ToErrors();
        }

        var mediaAssetDto = new GetMediaAssetDto
        {
            Id = mediaAsset.Id,
            FileName = mediaAsset.MediaData.FileName.Value,
            Status = mediaAsset.Status.ToString().ToLowerInvariant(),
            AssetType = mediaAsset.AssetType.ToString().ToLowerInvariant(),
            Context = mediaAsset.Owner.Context,
            ContextId = mediaAsset.Owner.EntityId,
            CreatedAt = mediaAsset.CreatedAt,
            UpdatedAt = mediaAsset.UpdatedAt,
            Size = mediaAsset.MediaData.Size,
            Url = null,
        };

        if (mediaAsset.Status == MediaStatus.UPLOADED)
        {
            var generateUrlResult = await fileStorageProvider.GenerateDownloadUrlAsync(mediaAsset.Key);
            if (generateUrlResult.IsFailure)
            {
                return generateUrlResult.Error.ToErrors();
            }

            mediaAssetDto = mediaAssetDto with { Url = generateUrlResult.Value };
        }

        return mediaAssetDto;
    }
}