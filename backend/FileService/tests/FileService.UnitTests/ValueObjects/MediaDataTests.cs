using FileService.Domain;

namespace FileService.UnitTests.ValueObjects;

public class MediaDataTests
{
    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        var result = MediaData.Create(
            CreateFileName(),
            CreateContentType(),
            1024,
            4);

        Assert.True(result.IsSuccess);
        Assert.Equal(1024, result.Value.Size);
        Assert.Equal(4, result.Value.ExpectedChunksCount);
        Assert.Equal("video.mp4", result.Value.FileName.Name);
        Assert.Equal("video/mp4", result.Value.ContentType.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1024)]
    public void Create_WithNonPositiveSize_ReturnsFailure(long size)
    {
        var result = MediaData.Create(
            CreateFileName(),
            CreateContentType(),
            size,
            4);

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithNonPositiveExpectedChunksCount_ReturnsFailure(int expectedChunksCount)
    {
        var result = MediaData.Create(
            CreateFileName(),
            CreateContentType(),
            1024,
            expectedChunksCount);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithValidZeroSizeChunks_DoesNotFailOnSize()
    {
        var result = MediaData.Create(
            CreateFileName(),
            CreateContentType(),
            2048,
            2);

        Assert.True(result.IsSuccess);
    }

    private static FileName CreateFileName(string name = "video.mp4") =>
        FileName.Create(name).Value;

    private static ContentType CreateContentType(string value = "video/mp4") =>
        ContentType.Create(value).Value;
}
