using FileService.Domain;

namespace FileService.UnitTests.ValueObjects;

public class FileNameTests
{
    [Fact]
    public void Create_WithValidFileName_ReturnsSuccess()
    {
        var result = FileName.Create("movie.mp4");

        Assert.True(result.IsSuccess);
        Assert.Equal("movie.mp4", result.Value.Value);
        Assert.Equal("mp4", result.Value.Extension);
    }

    [Theory]
    [InlineData("photo.JPG")]
    [InlineData("archive.Tar.Gz")]
    [InlineData("doc.PDF")]
    public void Create_WithValidFileName_ExtensionIsLowerCased(string fileName)
    {
        var result = FileName.Create(fileName);

        Assert.True(result.IsSuccess);
        Assert.Equal(fileName, result.Value.Value);
        Assert.Equal(fileName.Split('.').Last().ToLowerInvariant(), result.Value.Extension);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhiteSpace_ReturnsFailure(string? fileName)
    {
        var result = FileName.Create(fileName!);

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData("noextension")]
    [InlineData("trailingdot.")]
    public void Create_WithoutValidExtension_ReturnsFailure(string fileName)
    {
        var result = FileName.Create(fileName);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_FileWithDotInMiddle_ExtractsLastExtension()
    {
        var result = FileName.Create("archive.tar.gz");

        Assert.True(result.IsSuccess);
        Assert.Equal("gz", result.Value.Extension);
    }
}
