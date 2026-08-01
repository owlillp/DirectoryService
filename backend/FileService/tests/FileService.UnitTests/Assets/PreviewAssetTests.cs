using FileService.Domain;
using FileService.Domain.Assets;

namespace FileService.UnitTests.Assets;

public class PreviewAssetTests
{
    private const int ChunksCount = 4;

    private static readonly Guid ValidId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidMediaData_ReturnsSuccess()
    {
        var result = PreviewAsset.Validate(CreateValidMediaData());

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("photo.mp4")]
    [InlineData("photo.gif")]
    [InlineData("movie.mkv")]
    [InlineData("photo.bmp")]
    public void Validate_WithNotAllowedExtension_ReturnsFailure(string fileName)
    {
        var result = PreviewAsset.Validate(CreateValidMediaData(fileName));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Validate_WithNonImageContentType_ReturnsFailure()
    {
        var result = PreviewAsset.Validate(CreateValidMediaData(contentType: "video/mp4"));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Validate_WithSizeAboveMax_ReturnsFailure()
    {
        var result = PreviewAsset.Validate(CreateValidMediaData(size: PreviewAsset.MAX_SIZE + 1));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Validate_WithSizeExactlyAtMax_ReturnsSuccess()
    {
        var result = PreviewAsset.Validate(CreateValidMediaData(size: PreviewAsset.MAX_SIZE));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CreateForUpload_WithValidData_ReturnsUploadingAsset()
    {
        var result = PreviewAsset.CreateForUpload(ValidId, CreateValidMediaData(), CreateOwner());

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidId, result.Value.Id);
        Assert.Equal(AssetType.PREVIEW, result.Value.AssetType);
        Assert.Equal(MediaStatus.UPLOADING, result.Value.Status);
        Assert.Equal(PreviewAsset.LOCATION, result.Value.Key.Location);
        Assert.Equal(ValidId.ToString(), result.Value.Key.Key);
    }

    [Fact]
    public void CreateForUpload_WithInvalidMediaData_ReturnsFailure()
    {
        var result = PreviewAsset.CreateForUpload(ValidId, CreateValidMediaData(fileName: "photo.bmp"), CreateOwner());

        Assert.True(result.IsFailure);
    }

    private static MediaData CreateValidMediaData(string fileName = "photo.jpg", string contentType = "image/jpeg", long size = 1024)
    {
        var result = MediaData.Create(
            FileName.Create(fileName).Value,
            ContentType.Create(contentType).Value,
            size,
            ChunksCount);

        return result.Value;
    }

    private static MediaOwner CreateOwner() =>
        MediaOwner.ForUser(Guid.NewGuid()).Value;
}
