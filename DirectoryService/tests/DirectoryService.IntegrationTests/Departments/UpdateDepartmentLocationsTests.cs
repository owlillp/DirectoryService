using System.Net.Http.Json;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateDepartmentLocationsTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task UpdateDepartmentLocations_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var sourceLocation = await CreateLocationAsync("source_location", "source_country");
        var destinationLocation = await CreateLocationAsync("destination_location", "destination_country");

        var department = await CreateDepartmentAsync([sourceLocation.Id.Value]);

        var request = new UpdateDepartmentLocationsRequest([destinationLocation.Id.Value]);

        // act
        var updateDepartmentLocationsResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Departments/{department.Id.Value}/locations", request, cancellationToken);
        var updateDepartmentLocationsResult = await updateDepartmentLocationsResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert
        Assert.True(updateDepartmentLocationsResult.IsSuccess);
        Assert.NotEqual(Guid.Empty, updateDepartmentLocationsResult.Value);
        Assert.Equal(department.Id.Value, updateDepartmentLocationsResult.Value);

        await ExecuteInDb(async dbContext =>
        {
            var targetDepartment = await dbContext.Departments
                .Include(d => d.Locations)
                .FirstOrDefaultAsync(
                    d => d.Id == department.Id,
                    cancellationToken: cancellationToken);

            Assert.NotNull(targetDepartment);
            Assert.Single(targetDepartment.Locations);
            Assert.DoesNotContain(sourceLocation.Id, targetDepartment.Locations.Select(l => l.LocationId));
            Assert.Contains(destinationLocation.Id, targetDepartment.Locations.Select(l => l.LocationId));
        });
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_several_locations_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var sourceLocation = await CreateLocationAsync("source_location", "source_country");

        const int locationsLength = 3;
        var destinationLocations = new List<Location>();

        for (int i = 0; i < locationsLength; i++)
        {
            var destinationLocation = await CreateLocationAsync($"destination_location_{i}", $"destination_country_{i}");
            destinationLocations.Add(destinationLocation);
        }

        var department = await CreateDepartmentAsync([sourceLocation.Id.Value]);

        var request = new UpdateDepartmentLocationsRequest(destinationLocations.Select(l => l.Id.Value));

        // act
        var updateDepartmentLocationsResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Departments/{department.Id.Value}/locations", request, cancellationToken);
        var updateDepartmentLocationsResult = await updateDepartmentLocationsResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert
        Assert.True(updateDepartmentLocationsResult.IsSuccess);
        Assert.NotEqual(Guid.Empty, updateDepartmentLocationsResult.Value);
        Assert.Equal(department.Id.Value, updateDepartmentLocationsResult.Value);

        await ExecuteInDb(async dbContext =>
        {
            var targetDepartment = await dbContext.Departments
                .Include(d => d.Locations)
                .FirstOrDefaultAsync(
                    d => d.Id == department.Id,
                    cancellationToken: cancellationToken);

            Assert.NotNull(targetDepartment);
            Assert.Equal(locationsLength, targetDepartment.Locations.Count);
            Assert.DoesNotContain(sourceLocation.Id, targetDepartment.Locations.Select(l => l.LocationId));
            Assert.All(targetDepartment.Locations, dl => Assert.Contains(dl.LocationId, destinationLocations.Select(l => l.Id)));
        });
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_non_existent_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var destinationLocation = await CreateLocationAsync("destination_location", "destination_country");

        var departmentIdValue = Guid.NewGuid();

        var request = new UpdateDepartmentLocationsRequest([destinationLocation.Id.Value]);

        // act
        var updateDepartmentLocationsResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Departments/{departmentIdValue}/locations", request, cancellationToken);
        var updateDepartmentLocationsResult = await updateDepartmentLocationsResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(updateDepartmentLocationsResult.IsFailure);
        Assert.NotNull(updateDepartmentLocationsResult.Error);
        Assert.Contains(updateDepartmentLocationsResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_inactive_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var sourceLocation = await CreateLocationAsync("source_location", "source_country");
        var destinationLocation = await CreateLocationAsync("destination_location", "destination_country");

        var department = await CreateDepartmentAsync([sourceLocation.Id.Value], isActive: false);

        var request = new UpdateDepartmentLocationsRequest([destinationLocation.Id.Value]);

        // act
        var updateDepartmentLocationsResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Departments/{department.Id.Value}/locations", request, cancellationToken);
        var updateDepartmentLocationsResult = await updateDepartmentLocationsResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(updateDepartmentLocationsResult.IsFailure);
        Assert.NotNull(updateDepartmentLocationsResult.Error);
        Assert.Contains(updateDepartmentLocationsResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_null_locations_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var sourceLocation = await CreateLocationAsync("source_location", "source_country");

        var department = await CreateDepartmentAsync([sourceLocation.Id.Value]);

        var request = new UpdateDepartmentLocationsRequest(null!);

        // act
        var updateDepartmentLocationsResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Departments/{department.Id.Value}/locations", request, cancellationToken);
        var updateDepartmentLocationsResult = await updateDepartmentLocationsResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(updateDepartmentLocationsResult.IsFailure);
        Assert.NotNull(updateDepartmentLocationsResult.Error);
        Assert.Contains(updateDepartmentLocationsResult.Error, e => e.Type == ErrorType.FAILURE);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_empty_locations_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var sourceLocation = await CreateLocationAsync("source_location", "source_country");

        var department = await CreateDepartmentAsync([sourceLocation.Id.Value]);

        var request = new UpdateDepartmentLocationsRequest([]);

        // act
        var updateDepartmentLocationsResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Departments/{department.Id.Value}/locations", request, cancellationToken);
        var updateDepartmentLocationsResult = await updateDepartmentLocationsResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(updateDepartmentLocationsResult.IsFailure);
        Assert.NotNull(updateDepartmentLocationsResult.Error);
        Assert.Contains(updateDepartmentLocationsResult.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_non_existent_locations_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var sourceLocation = await CreateLocationAsync("source_location", "source_country");
        var destinationLocationIdValue = Guid.NewGuid();

        var department = await CreateDepartmentAsync([sourceLocation.Id.Value]);

        var request = new UpdateDepartmentLocationsRequest([destinationLocationIdValue]);

        // act
        var updateDepartmentLocationsResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Departments/{department.Id.Value}/locations", request, cancellationToken);
        var updateDepartmentLocationsResult = await updateDepartmentLocationsResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(updateDepartmentLocationsResult.IsFailure);
        Assert.NotNull(updateDepartmentLocationsResult.Error);
        Assert.Contains(updateDepartmentLocationsResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_inactive_locations_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var sourceLocation = await CreateLocationAsync("source_location", "source_country");
        var destinationLocation = await CreateLocationAsync("destination_location", "destination_country", isActive: false);

        var department = await CreateDepartmentAsync([sourceLocation.Id.Value]);

        var request = new UpdateDepartmentLocationsRequest([destinationLocation.Id.Value]);

        // act
        var updateDepartmentLocationsResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Departments/{department.Id.Value}/locations", request, cancellationToken);
        var updateDepartmentLocationsResult = await updateDepartmentLocationsResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(updateDepartmentLocationsResult.IsFailure);
        Assert.NotNull(updateDepartmentLocationsResult.Error);
        Assert.Contains(updateDepartmentLocationsResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_duplicate_locations_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var sourceLocation = await CreateLocationAsync("source_location", "source_country");
        var destinationLocation = await CreateLocationAsync("destination_location", "destination_country");

        var department = await CreateDepartmentAsync([sourceLocation.Id.Value]);

        var request = new UpdateDepartmentLocationsRequest([destinationLocation.Id.Value, destinationLocation.Id.Value]);

        // act
        var updateDepartmentLocationsResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Departments/{department.Id.Value}/locations", request, cancellationToken);
        var updateDepartmentLocationsResult = await updateDepartmentLocationsResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(updateDepartmentLocationsResult.IsFailure);
        Assert.NotNull(updateDepartmentLocationsResult.Error);
        Assert.Contains(updateDepartmentLocationsResult.Error, e => e.Type == ErrorType.VALIDATION);
    }
}