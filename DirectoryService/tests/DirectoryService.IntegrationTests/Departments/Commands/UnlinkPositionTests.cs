using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Departments.Commands;

public class UnlinkPositionTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task UnlinkPosition_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([department.Id.Value], name: "test_pos");

        // act
        var response = await AppHttpClient.DeleteAsync(
            $"/api/Departments/{department.Id.Value}/positions/{position.Id.Value}",
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            bool exists = await dbContext.DepartmentPositions
                .AnyAsync(dp => dp.DepartmentId == department.Id && dp.PositionId == position.Id, cancellationToken);
            Assert.False(exists);
        });
    }

    [Fact]
    public async Task UnlinkPosition_when_link_not_exists_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([], name: "test_pos");

        // act
        var response = await AppHttpClient.DeleteAsync(
            $"/api/Departments/{department.Id.Value}/positions/{position.Id.Value}",
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UnlinkPosition_with_non_existent_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([department.Id.Value], name: "test_pos");

        // act
        var response = await AppHttpClient.DeleteAsync(
            $"/api/Departments/{Guid.NewGuid()}/positions/{position.Id.Value}",
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UnlinkPosition_with_invalid_guid_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var response = await AppHttpClient.DeleteAsync(
            $"/api/Departments/{Guid.Empty}/positions/{Guid.Empty}",
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }
}
