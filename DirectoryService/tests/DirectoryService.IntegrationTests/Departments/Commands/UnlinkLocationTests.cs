using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace DirectoryService.IntegrationTests.Departments.Commands;

public class UnlinkLocationTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task UnlinkLocation_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        // act
        var response = await AppHttpClient.DeleteAsync(
            $"/api/Departments/{department.Id.Value}/locations/{location.Id.Value}",
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            bool exists = await dbContext.DepartmentLocations
                .AnyAsync(dl => dl.DepartmentId == department.Id && dl.LocationId == location.Id, cancellationToken);
            Assert.False(exists);
        });
    }

    [Fact]
    public async Task UnlinkLocation_when_link_not_exists_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var initialLocation = await CreateLocationAsync(name: "initial", city: "initial");
        var department = await CreateDepartmentAsync([initialLocation.Id.Value]);

        // act
        var response = await AppHttpClient.DeleteAsync(
            $"/api/Departments/{department.Id.Value}/locations/{location.Id.Value}",
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UnlinkLocation_with_non_existent_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        // act
        var response = await AppHttpClient.DeleteAsync(
            $"/api/Departments/{Guid.NewGuid()}/locations/{location.Id.Value}",
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UnlinkLocation_with_invalid_guid_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var response = await AppHttpClient.DeleteAsync(
            $"/api/Departments/{Guid.Empty}/locations/{Guid.Empty}",
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }
}
