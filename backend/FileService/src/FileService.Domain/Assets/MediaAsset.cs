using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace FileService.Domain.Assets;

public abstract class MediaAsset
{
    public Guid Id { get; protected set; }

    public MediaData MediaData { get; protected set; } = null!;

    public AssetType AssetType { get; protected set; }

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    public StorageKey Key { get; protected set; } = null!;

    public MediaOwner Owner { get; protected set; } = null!;

    public MediaStatus Status { get; protected set; }

    // EF Core
    protected MediaAsset() { }

    protected MediaAsset(
        Guid id,
        MediaData mediaData,
        AssetType assetType,
        MediaOwner owner,
        MediaStatus status,
        StorageKey key)
    {
        Id = id;
        MediaData = mediaData;
        AssetType = assetType;
        Owner = owner;
        Status = status;
        Key = key;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<MediaAsset, Error> CreateForUpload(MediaData mediaData, AssetType assetType, MediaOwner owner)
    {
        var assetId = Guid.NewGuid();

        switch (assetType)
        {
            case AssetType.VIDEO:
                var videoResult = VideoAsset.CreateForUpload(assetId, mediaData, owner);
                return videoResult.IsFailure ? videoResult.Error : videoResult.Value;
            case AssetType.PREVIEW:
                var previewResult = PreviewAsset.CreateForUpload(assetId, mediaData, owner);
                return previewResult.IsFailure ? previewResult.Error : previewResult.Value;
            default:
                throw new ArgumentOutOfRangeException(nameof(assetType), assetType, null);
        }
    }

    public UnitResult<Error> MarkUploaded()
    {
        if (Status != MediaStatus.UPLOADING)
            return GeneralErrors.ValueIsInvalid("status");

        Status = MediaStatus.UPLOADED;
        UpdatedAt = DateTime.UtcNow;
        return UnitResult.Success<Error>();
    }

    public void MarkFailed()
    {
        Status = MediaStatus.FAILED;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDeleted()
    {
        Status = MediaStatus.DELETED;
        UpdatedAt = DateTime.UtcNow;
    }
}