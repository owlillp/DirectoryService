using System.Net.Http.Json;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace DirectoryService.IntegrationTests.Departments.Commands;

public class LinkPositionTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task LinkPosition_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([], name: "test_pos");

        // act
        var response = await AppHttpClient.PostAsJsonAsync<object?>(
            $"/api/Departments/{department.Id.Value}/positions/{position.Id.Value}",
            null,
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            bool exists = await dbContext.DepartmentPositions
                .AnyAsync(dp => dp.DepartmentId == department.Id && dp.PositionId == position.Id, cancellationToken);
            Assert.True(exists);
        });
    }

    [Fact]
    public async Task LinkPosition_when_position_already_linked_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([department.Id.Value]);

        // act
        var response = await AppHttpClient.PostAsJsonAsync<object?>(
            $"/api/Departments/{department.Id.Value}/positions/{position.Id.Value}",
            null,
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.CONFLICT);
    }

    [Fact]
    public async Task LinkPosition_with_non_existent_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var position = await CreatePositionAsync([], name: "test_pos");

        // act
        var response = await AppHttpClient.PostAsJsonAsync<object?>(
            $"/api/Departments/{Guid.NewGuid()}/positions/{position.Id.Value}",
            null,
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task LinkPosition_with_non_existent_position_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        // act
        var response = await AppHttpClient.PostAsJsonAsync<object?>(
            $"/api/Departments/{department.Id.Value}/positions/{Guid.NewGuid()}",
            null,
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task LinkPosition_deleted_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], isActive: false);
        var position = await CreatePositionAsync([], name: "test_pos");

        // act
        var response = await AppHttpClient.PostAsJsonAsync<object?>(
            $"/api/Departments/{department.Id.Value}/positions/{position.Id.Value}",
            null,
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task LinkPosition_with_invalid_guid_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var response = await AppHttpClient.PostAsJsonAsync<object?>(
            $"/api/Departments/{Guid.Empty}/positions/{Guid.Empty}",
            null,
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }
}
