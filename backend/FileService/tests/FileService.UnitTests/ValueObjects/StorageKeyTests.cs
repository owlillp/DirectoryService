using FileService.Domain;

namespace FileService.UnitTests.ValueObjects;

public class StorageKeyTests
{
    [Fact]
    public void Create_WithLocationAndKey_ReturnsSuccess()
    {
        var result = StorageKey.Create("videos", null, "abc-123");

        Assert.True(result.IsSuccess);
        Assert.Equal("videos", result.Value.Location);
        Assert.Equal("abc-123", result.Value.Key);
        Assert.Equal(string.Empty, result.Value.Prefix);
        Assert.Equal("abc-123", result.Value.Value);
        Assert.Equal("videos/abc-123", result.Value.FullPath);
    }

    [Fact]
    public void Create_WithEmptyPrefix_ValueEqualsKey()
    {
        var result = StorageKey.Create("pictures", string.Empty, "photo-id");

        Assert.True(result.IsSuccess);
        Assert.Equal("photo-id", result.Value.Value);
        Assert.Equal("pictures/photo-id", result.Value.FullPath);
    }

    [Fact]
    public void Create_WithPrefix_BuildsValueAndFullPath()
    {
        var result = StorageKey.Create("videos", "raw", "abc-123");

        Assert.True(result.IsSuccess);
        Assert.Equal("raw", result.Value.Prefix);
        Assert.Equal("raw/abc-123", result.Value.Value);
        Assert.Equal("videos/raw/abc-123", result.Value.FullPath);
    }

    [Fact]
    public void Create_WithNestedPrefix_NormalizesSegments()
    {
        var result = StorageKey.Create("videos", "raw/2024", "abc-123");

        Assert.True(result.IsSuccess);
        Assert.Equal("raw/2024", result.Value.Prefix);
        Assert.Equal("raw/2024/abc-123", result.Value.Value);
    }

    [Fact]
    public void Create_WithBackslashPrefix_NormalizesToForwardSlash()
    {
        var result = StorageKey.Create("videos", @"raw\2024", "abc-123");

        Assert.True(result.IsSuccess);
        Assert.Equal("raw/2024", result.Value.Prefix);
    }

    [Fact]
    public void Create_WithEmptyPrefixParts_RemovesEmptySegments()
    {
        var result = StorageKey.Create("videos", "raw//2024", "abc-123");

        Assert.True(result.IsSuccess);
        Assert.Equal("raw/2024", result.Value.Prefix);
        Assert.Equal("raw/2024/abc-123", result.Value.Value);
    }

    [Fact]
    public void Create_WithPrefixSurroundedBySpaces_TrimsParts()
    {
        var result = StorageKey.Create("videos", " raw / 2024 ", "abc-123");

        Assert.True(result.IsSuccess);
        Assert.Equal("raw/2024", result.Value.Prefix);
    }

    [Fact]
    public void Create_WithLocationSurroundingSpaces_TrimsLocation()
    {
        var result = StorageKey.Create("  videos  ", null, "abc-123");

        Assert.True(result.IsSuccess);
        Assert.Equal("videos", result.Value.Location);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhiteSpaceLocation_ReturnsFailure(string? location)
    {
        var result = StorageKey.Create(location!, null, "abc-123");

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhiteSpaceKey_ReturnsFailure(string? key)
    {
        var result = StorageKey.Create("videos", null, key!);

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData("nested/key")]
    [InlineData(@"nested\key")]
    public void Create_WithSlashInKey_ReturnsFailure(string key)
    {
        var result = StorageKey.Create("videos", null, key);

        Assert.True(result.IsFailure);
    }
}
