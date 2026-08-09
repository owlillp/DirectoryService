using FileService.Domain;

namespace FileService.Application.Models;

public static class MediaAssetCacheKeys
{
    public const string ItemPrefix = "media_asset:item";
    public const string PresignedUrlPrefix = "presigned_url:item";

    private const string Version = "v1";

    public static string BuildItemKey(Guid mediaAssetId)
        => $"{ItemPrefix}:{Version}:{mediaAssetId}";

    public static string BuildPresignedUrlKey(StorageKey storageKey)
        => $"{PresignedUrlPrefix}:{Version}:{storageKey.FullPath}";
}