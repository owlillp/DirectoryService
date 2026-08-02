namespace FileService.Domain;

public enum AssetType
{
    VIDEO,
    PREVIEW,
    AVATAR,
}

public static class AssetTypeExtensions
{
    extension(string value)
    {
        public AssetType ToAssetType()
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Invalid asset type: {value}");
            }

            return value.ToLowerInvariant() switch
            {
                "video" => AssetType.VIDEO,
                "preview" => AssetType.PREVIEW,
                "avatar" => AssetType.AVATAR,
                _ => throw new ArgumentException($"Invalid asset type: {value}")
            };
        }

        public bool IsAssetType()
            => value is "video" or "preview" or "avatar";
    }
}