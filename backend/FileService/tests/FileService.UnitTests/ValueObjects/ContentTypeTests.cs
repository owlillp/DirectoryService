using FileService.Domain;

namespace FileService.UnitTests.ValueObjects;

public class ContentTypeTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhiteSpace_ReturnsFailure(string? contentType)
    {
        var result = ContentType.Create(contentType!);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithVideoContentType_ReturnsVideoCategory()
    {
        var result = ContentType.Create("video/mp4");

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaType.VIDEO, result.Value.Category);
        Assert.Equal("video/mp4", result.Value.Value);
    }

    [Fact]
    public void Create_WithAudioContentType_ReturnsAudioCategory()
    {
        var result = ContentType.Create("audio/mpeg");

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaType.AUDIO, result.Value.Category);
    }

    [Fact]
    public void Create_WithImageContentType_ReturnsImageCategory()
    {
        var result = ContentType.Create("image/png");

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaType.IMAGE, result.Value.Category);
    }

    [Fact]
    public void Create_WithDocumentContentType_ReturnsDocumentCategory()
    {
        var result = ContentType.Create("application/document/pdf");

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaType.DOCUMENT, result.Value.Category);
    }

    [Fact]
    public void Create_WithUnknownContentType_ReturnsUnknownCategory()
    {
        var result = ContentType.Create("application/octet-stream");

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaType.UNKNOWN, result.Value.Category);
    }

    [Theory]
    [InlineData("VIDEO/MP4", MediaType.VIDEO)]
    [InlineData("Image/jpeg", MediaType.IMAGE)]
    [InlineData("AUDIO/mp3", MediaType.AUDIO)]
    public void Create_WithMixedCaseContentType_IsCaseInsensitive(string contentType, MediaType expected)
    {
        var result = ContentType.Create(contentType);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Category);
    }
}
