using FileService.Domain;

namespace FileService.UnitTests.ValueObjects;

public class AssetTypeTests
{
    [Fact]
    public void ToAssetType_WithVideo_ReturnsVideo()
    {
        Assert.Equal(AssetType.VIDEO, "video".ToAssetType());
    }

    [Fact]
    public void ToAssetType_WithPreview_ReturnsPreview()
    {
        Assert.Equal(AssetType.PREVIEW, "preview".ToAssetType());
    }

    [Fact]
    public void ToAssetType_WithAvatar_ReturnsAvatar()
    {
        Assert.Equal(AssetType.AVATAR, "avatar".ToAssetType());
    }

    [Theory]
    [InlineData("")]
    [InlineData("unknown")]
    [InlineData(null)]
    public void ToAssetType_WithInvalidValue_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => value!.ToAssetType());
    }
}
