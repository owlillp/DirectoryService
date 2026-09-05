using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Application.Abstractions;
using FileService.Contracts.Files.Dtos;
using FileService.Contracts.Files.Responses;
using FileService.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel.Failures;

namespace FileService.Application.Features.Queries.GetMediaAssetsForEntity;

public class GetMediaAssetsForEntityHandler(
    IValidator<GetMediaAssetsForEntityQuery> validator,
    IFileStorageProvider fileStorageProvider,
    IReadDbContext readDbContext)
    : IQueryHandler<GetFilesForEntityResponse, GetMediaAssetsForEntityQuery>
{
    public async Task<Result<GetFilesForEntityResponse, Errors>> Handle(
        GetMediaAssetsForEntityQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var request = query.Request;

        var mediaAssets = await readDbContext.MediaAssetsRead
            .Where(ma => ma.Owner.Context == request.Context && ma.Owner.EntityId == request.EntityId)
            .ToListAsync(cancellationToken);

        var uploadedAssetKeys = mediaAssets
            .Where(ma => ma.Status == MediaStatus.READY)
            .Select(ma => ma.UploadKey)
            .ToList();

        var getUrlsResult = await fileStorageProvider.GenerateDownloadUrlsAsync(uploadedAssetKeys!, cancellationToken);
        if (getUrlsResult.IsFailure)
        {
            return getUrlsResult.Error.ToErrors();
        }

        var urlsDict = getUrlsResult.Value.ToDictionary(url => url.StorageKey, url => url.PresignedUrl);

        var mediaAssetDtos = new List<GetMediaAssetsDto>();
        foreach (var mediaAsset in mediaAssets)
        {
            urlsDict.TryGetValue(mediaAsset.UploadKey, out string? url);

            mediaAssetDtos.Add(new GetMediaAssetsDto
            {
                Id = mediaAsset.Id,
                FileName = mediaAsset.MediaData.FileName.Value,
                Status = mediaAsset.Status.ToString().ToLowerInvariant(),
                AssetType = mediaAsset.AssetType.ToString().ToLowerInvariant(),
                Url = url,
            });
        }

        return new GetFilesForEntityResponse(request.Context, request.EntityId, mediaAssetDtos.ToArray());
    }
}