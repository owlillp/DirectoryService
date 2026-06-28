using System.Net.Http.Json;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace DirectoryService.IntegrationTests.Departments.Commands;

public class LinkLocationTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task LinkLocation_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var initialLocation = await CreateLocationAsync(name: "initial", city: "initial");
        var department = await CreateDepartmentAsync([initialLocation.Id.Value]);

        // act
        var response = await AppHttpClient.PostAsJsonAsync<object?>(
            $"/api/Departments/{department.Id.Value}/locations/{location.Id.Value}",
            null,
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            bool exists = await dbContext.DepartmentLocations
                .AnyAsync(dl => dl.DepartmentId == department.Id && dl.LocationId == location.Id, cancellationToken);
            Assert.True(exists);
        });
    }

    [Fact]
    public async Task LinkLocation_when_location_already_linked_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        // act
        var response = await AppHttpClient.PostAsJsonAsync<object?>(
            $"/api/Departments/{department.Id.Value}/locations/{location.Id.Value}",
            null,
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.CONFLICT);
    }

    [Fact]
    public async Task LinkLocation_with_non_existent_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        // act
        var response = await AppHttpClient.PostAsJsonAsync<object?>(
            $"/api/Departments/{Guid.NewGuid()}/locations/{location.Id.Value}",
            null,
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task LinkLocation_deleted_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var initialLocation = await CreateLocationAsync(name: "initial", city: "initial");
        var department = await CreateDepartmentAsync([initialLocation.Id.Value], isActive: false);

        // act
        var response = await AppHttpClient.PostAsJsonAsync<object?>(
            $"/api/Departments/{department.Id.Value}/locations/{location.Id.Value}",
            null,
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task LinkLocation_with_non_existent_location_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        // act
        var response = await AppHttpClient.PostAsJsonAsync<object?>(
            $"/api/Departments/{department.Id.Value}/locations/{Guid.NewGuid()}",
            null,
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task LinkLocation_with_invalid_guid_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var response = await AppHttpClient.PostAsJsonAsync<object?>(
            $"/api/Departments/{Guid.Empty}/locations/{Guid.Empty}",
            null,
            cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }
}
