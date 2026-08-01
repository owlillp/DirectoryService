using FileService.Domain;

namespace FileService.UnitTests.ValueObjects;

public class MediaOwnerTests
{
    private static readonly Guid ValidId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidContextAndId_ReturnsSuccess()
    {
        var result = MediaOwner.Create("location", ValidId);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidId, result.Value.EntityId);
    }

    [Fact]
    public void Create_WithMixedCaseContext_NormalizesToLowercase()
    {
        var result = MediaOwner.Create("LOCATION", ValidId);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidId, result.Value.EntityId);
        Assert.Equal("LOCATION", result.Value.Context);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhiteSpaceContext_ReturnsFailure(string? context)
    {
        var result = MediaOwner.Create(context!, ValidId);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithContextLongerThan50Chars_ReturnsFailure()
    {
        var longContext = new string('a', 51);

        var result = MediaOwner.Create(longContext, ValidId);

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("userr")]
    [InlineData("owner")]
    public void Create_WithNotAllowedContext_ReturnsFailure(string context)
    {
        var result = MediaOwner.Create(context, ValidId);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithEmptyGuid_ReturnsFailure()
    {
        var result = MediaOwner.Create("user", Guid.Empty);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ForLocation_CreatesContextLocation()
    {
        var result = MediaOwner.ForLocation(ValidId);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidId, result.Value.EntityId);
    }

    [Fact]
    public void ForPosition_CreatesContextPosition()
    {
        var result = MediaOwner.ForPosition(ValidId);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidId, result.Value.EntityId);
    }

    [Fact]
    public void ForDepartment_CreatesContextDepartment()
    {
        var result = MediaOwner.ForDepartment(ValidId);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidId, result.Value.EntityId);
    }

    [Fact]
    public void ForUser_CreatesContextUser()
    {
        var result = MediaOwner.ForUser(ValidId);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidId, result.Value.EntityId);
    }

    [Fact]
    public void ForUser_WithEmptyGuid_ReturnsFailure()
    {
        var result = MediaOwner.ForUser(Guid.Empty);

        Assert.True(result.IsFailure);
    }
}
