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
    }

    protected UnitResult<Error> SetStatus(MediaStatus status)
    {
        if (status < Status)
            return GeneralErrors.ValueIsInvalid(nameof(status));

        Status = status;
        return UnitResult.Success<Error>();
    }
}