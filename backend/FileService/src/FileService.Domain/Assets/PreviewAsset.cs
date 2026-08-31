using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace FileService.Domain.Assets;

public class PreviewAsset : MediaAsset
{
    public const long MAX_SIZE = 5_368_709;

    public const string LOCATION = "pictures";
    public const string RAW_PREFIX = "raw";
    public const string ALLOWED_CONTENT_TYPE = "image";

    public static readonly string[] AllowedExtensions = ["jpg", "jpeg", "png", "webp"];

    // EF Core
    private PreviewAsset() { }

    private PreviewAsset(Guid id, MediaData mediaData, MediaOwner owner, MediaStatus status, StorageKey key)
        : base(id, mediaData, AssetType.PREVIEW, owner, status, key, true)
    { }

    public static UnitResult<Error> Validate(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
            return GeneralErrors.ValueIsInvalid(nameof(mediaData.FileName.Extension));

        if(mediaData.ContentType.Category != MediaType.IMAGE)
            return GeneralErrors.ValueIsInvalid(nameof(mediaData.ContentType.Category));

        if(mediaData.Size > MAX_SIZE)
            return GeneralErrors.ValueIsInvalid(nameof(mediaData.Size));

        return UnitResult.Success<Error>();
    }

    public static Result<PreviewAsset, Error> CreateForUpload(Guid id, MediaData mediaData, MediaOwner owner)
    {
        var validation = Validate(mediaData);
        if (validation.IsFailure)
            return validation.Error;

        var key = StorageKey.Create(LOCATION, null, id.ToString());
        if(key.IsFailure)
            return key.Error;

        return new PreviewAsset(id, mediaData, owner, MediaStatus.UPLOADING, key.Value);
    }
}