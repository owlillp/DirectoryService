using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Application.Models;
using FileService.Contracts.Files.Dtos;
using FileService.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Queries.GetMediaAsset;

public class GetMediaAssetHandler(
    IValidator<GetMediaAssetQuery> validator,
    IFileStorageProvider fileStorageProvider,
    HybridCache cache,
    IOptions<FileStorageOptions> fileStorageOptions,
    IReadDbContext readDbContext) : IQueryHandler<GetMediaAssetDto, GetMediaAssetQuery>
{
    private readonly FileStorageOptions _fileStorageOptions = fileStorageOptions.Value;

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
            string? presignedUrl = await GetPresignedUrlFromCacheAsync(mediaAsset.Key, cancellationToken);
            mediaAssetDto = mediaAssetDto with { Url = presignedUrl };
        }

        return mediaAssetDto;
    }

    private async Task<string?> GetPresignedUrlFromCacheAsync(StorageKey storageKey, CancellationToken cancellationToken)
    {
        string key = MediaAssetCacheKeys.BuildPresignedUrlKey(storageKey);

        return await cache.GetOrCreateAsync<string?>(
            key,
            async _ =>
            {
                var generateUrlResult = await fileStorageProvider.GenerateDownloadUrlAsync(storageKey);
                return generateUrlResult.IsSuccess
                    ? generateUrlResult.Value
                    : null;
            },
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(_fileStorageOptions.DownloadExpirationHours).Subtract(TimeSpan.FromHours(1)),
                LocalCacheExpiration = TimeSpan.FromHours(1),
            },
            cancellationToken: cancellationToken);
    }
}