using FileService.Domain;
using FileService.Domain.Assets;

namespace FileService.UnitTests.Assets;

public class VideoAssetTests
{
    private static readonly Guid ValidId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidMediaData_ReturnsSuccess()
    {
        var result = VideoAsset.Validate(CreateValidMediaData());

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("movie.png")]
    [InlineData("movie.gif")]
    [InlineData("movie.docx")]
    [InlineData("movie.mp3")]
    public void Validate_WithNotAllowedExtension_ReturnsFailure(string fileName)
    {
        var result = VideoAsset.Validate(CreateValidMediaData(fileName));

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData("image/png")]
    [InlineData("audio/mp3")]
    [InlineData("application/octet-stream")]
    public void Validate_WithNonVideoContentType_ReturnsFailure(string contentType)
    {
        var result = VideoAsset.Validate(CreateValidMediaData(contentType: contentType));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Validate_WithSizeAboveMax_ReturnsFailure()
    {
        var result = VideoAsset.Validate(CreateValidMediaData(size: VideoAsset.MAX_SIZE + 1));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Validate_WithSizeExactlyAtMax_ReturnsSuccess()
    {
        var result = VideoAsset.Validate(CreateValidMediaData(size: VideoAsset.MAX_SIZE));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CreateForUpload_WithValidData_ReturnsUploadingAsset()
    {
        var result = VideoAsset.CreateForUpload(ValidId, CreateValidMediaData(), CreateOwner());

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidId, result.Value.Id);
        Assert.Equal(AssetType.VIDEO, result.Value.AssetType);
        Assert.Equal(MediaStatus.UPLOADING, result.Value.Status);
        Assert.Equal(VideoAsset.LOCATION, result.Value.Key.Location);
        Assert.Equal(ValidId.ToString(), result.Value.Key.Key);
    }

    [Fact]
    public void CreateForUpload_WithInvalidMediaData_ReturnsFailure()
    {
        var result = VideoAsset.CreateForUpload(ValidId, CreateValidMediaData(fileName: "movie.png"), CreateOwner());

        Assert.True(result.IsFailure);
    }

    private static MediaData CreateValidMediaData(string fileName = "movie.mp4", string contentType = "video/mp4", long size = 1024)
    {
        var result = MediaData.Create(
            FileName.Create(fileName).Value,
            ContentType.Create(contentType).Value,
            size,
            4);

        return result.Value;
    }

    private static MediaOwner CreateOwner() =>
        MediaOwner.ForUser(Guid.NewGuid()).Value;
}
