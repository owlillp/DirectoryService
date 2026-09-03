using CSharpFunctionalExtensions;
using Shared.SharedKernel.Failures;

namespace FileService.Domain.Assets;

public class VideoAsset : MediaAsset
{
    private readonly List<StorageKey> _previewKeys = new();

    public const long MAX_SIZE = 5_368_709_120;

    public const string LOCATION = "videos";
    public const string RAW_PREFIX = "raw";
    public const string HLS_FOLDER = "hls";
    public const string HLS_ROOT_PREFIX = "hls";
    public const string MASTER_PLAYLIST_NAME = "master.m3u8";
    public const string STREAM_PLAYLIST_PATTERN = "%v_stream.m3u8";
    public const string SEGMENT_FILE_PATTERN = "%v_%06d.ts";
    public const string ALLOWED_CONTENT_TYPE = "video";

    public static readonly string[] AllowedExtensions = ["mp4", "mkv", "avi", "mov"];

    public StorageKey? SpritePreviewKey { get; private set; }

    public VideoMetadata? Metadata { get; private set; }

    public IReadOnlyList<StorageKey> PreviewKeys => _previewKeys.AsReadOnly();

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

    public Result<StorageKey, Error> GetHlsRootKey()
        => StorageKey.Create(LOCATION, HLS_ROOT_PREFIX, Id.ToString());

    public Result<StorageKey, Error> GetHlsMasterPlaylistKey()
    {
        var hlsRootResult = GetHlsRootKey();
        if (hlsRootResult.IsFailure)
        {
            return hlsRootResult.Error;
        }

        return hlsRootResult.Value.AppendKey(MASTER_PLAYLIST_NAME);
    }

    public UnitResult<Error> SetHlsMasterPlaylistKey(StorageKey value)
    {
        if (Status != MediaStatus.PROCESSING)
        {
            return Error.Validation("video.invalid.status", "Can only set processed data during processing");
        }

        Key = value;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

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

    public UnitResult<Error> CompleteProcessing()
    {
        if (Status != MediaStatus.PROCESSING)
        {
            return Error.Validation("asset.invalid.status.transition", "Can only complete processing from PROCESSING status");
        }

        Status = MediaStatus.READY;
        UpdatedAt = DateTime.UtcNow;
        return UnitResult.Success<Error>();
    }

    public void SetPreviewKeys(IEnumerable<StorageKey> previewKeys, StorageKey? spriteKey = null)
    {
        _previewKeys.Clear();
        _previewKeys.AddRange(previewKeys);
        SpritePreviewKey = spriteKey;
        UpdatedAt = DateTime.UtcNow;
    }
}