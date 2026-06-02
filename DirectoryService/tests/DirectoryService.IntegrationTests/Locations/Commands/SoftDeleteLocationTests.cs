using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace DirectoryService.IntegrationTests.Locations.Commands;

public class SoftDeleteLocationTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task SoftDeleteLocation_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        // act
        var response = await AppHttpClient.DeleteAsync($"/api/Locations/{location.Id.Value}", cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var targetLocation = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == location.Id, cancellationToken);

            Assert.NotNull(targetLocation);
            Assert.False(targetLocation.IsActive);
            Assert.NotNull(targetLocation.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeleteLocation_when_location_not_found_should_return_not_found_error()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var nonExistentId = Guid.NewGuid();

        // act
        var response = await AppHttpClient.DeleteAsync($"/api/Locations/{nonExistentId}", cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task SoftDeleteLocation_when_location_already_deleted_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var firstDeleteResponse = await AppHttpClient.DeleteAsync($"/api/Locations/{location.Id.Value}", cancellationToken);
        var firstDeleteResult = await firstDeleteResponse.HandleResponseAsync(cancellationToken);
        Assert.True(firstDeleteResult.IsSuccess);

        // act
        var secondDeleteResponse = await AppHttpClient.DeleteAsync($"/api/Locations/{location.Id.Value}", cancellationToken);
        var secondDeleteResult = await secondDeleteResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(secondDeleteResult.IsFailure);
        Assert.NotNull(secondDeleteResult.Error);
        Assert.Contains(secondDeleteResult.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task SoftDeleteLocation_with_invalid_guid_should_return_validation_error()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var invalidId = Guid.Empty;

        // act
        var response = await AppHttpClient.DeleteAsync($"/api/Locations/{invalidId}", cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }
}