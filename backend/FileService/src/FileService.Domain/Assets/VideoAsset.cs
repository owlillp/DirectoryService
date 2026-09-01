using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace FileService.Domain.Assets;

public class VideoAsset : MediaAsset
{
    public const long MAX_SIZE = 5_368_709_120;

    public const string LOCATION = "videos";
    public const string RAW_PREFIX = "raw";
    public const string HLS_FOLDER = "hls";
    public const string MASTER_PLAYLIST_NAME = "master.m3u8";
    public const string ALLOWED_CONTENT_TYPE = "video";

    public static readonly string[] AllowedExtensions = ["mp4", "mkv", "avi", "mov"];

    public VideoMetadata? Metadata { get; private set; }

    // EF Core
    private VideoAsset() { }

    private VideoAsset(Guid id, MediaData mediaData, MediaOwner owner, MediaStatus status, StorageKey key)
        : base(id, mediaData, AssetType.VIDEO, owner, status, key)
    { }

    public static UnitResult<Error> Validate(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
            return GeneralErrors.ValueIsInvalid(nameof(mediaData.FileName.Extension));

        if(mediaData.ContentType.Category != MediaType.VIDEO)
            return GeneralErrors.ValueIsInvalid(nameof(mediaData.ContentType.Category));

        if(mediaData.Size > MAX_SIZE)
            return GeneralErrors.ValueIsInvalid(nameof(mediaData.Size));

        return UnitResult.Success<Error>();
    }

    public static Result<VideoAsset, Error> CreateForUpload(Guid id, MediaData mediaData, MediaOwner owner)
    {
        var validation = Validate(mediaData);
        if (validation.IsFailure)
            return validation.Error;

        var key = StorageKey.Create(LOCATION, null, id.ToString());
        if(key.IsFailure)
            return key.Error;

        return new VideoAsset(id, mediaData, owner, MediaStatus.UPLOADING, key.Value);
    }

    public void SetMetadata(VideoMetadata metadata)
        => Metadata = metadata;

    public override bool RequiresProcessing() => true;

    public UnitResult<Error> StartProcessing()
    {
        if (Status != MediaStatus.UPLOADED)
        {
            return Error.Validation("asset.invalid.status.transition", "Can only start processing from UPLOADED status");
        }

        if (!RequiresProcessing())
        {
            return Error.Validation("asset.processing.not.required", "This asset type does not require processing");
        }

        Status = MediaStatus.PROCESSING;
        UpdatedAt = DateTime.UtcNow;
        return UnitResult.Success<Error>();
    }
}