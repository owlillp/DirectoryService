using DirectoryService.Contracts.Locations.Dtos;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Locations.Queries;

public class GetLocationTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task GetLocation_should_return_location_with_department_ids()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync("test_loc");
        var department = await CreateDepartmentAsync([location.Id.Value]);

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Locations/{location.Id.Value}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<LocationDto>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(location.Id.Value, result.Value.Id);
        Assert.Equal("test_loc", result.Value.Name);
        Assert.True(result.Value.IsActive);
        Assert.Single(result.Value.DepartmentIds);
        Assert.Contains(department.Id.Value, result.Value.DepartmentIds);
        Assert.Equal("test_country", result.Value.Address.Country);
    }

    [Fact]
    public async Task GetLocation_without_department_should_return_empty_department_ids()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync("orphan_loc", "orphan_country");

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Locations/{location.Id.Value}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<LocationDto>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.DepartmentIds);
    }

    [Fact]
    public async Task GetLocation_should_return_full_address()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync(
            name: "addr_loc",
            country: "Country",
            city: "City",
            street: "Street",
            postalCode: 123,
            buildingNumber: 10);

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Locations/{location.Id.Value}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<LocationDto>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Country", result.Value.Address.Country);
        Assert.Equal("City", result.Value.Address.City);
        Assert.Equal("Street", result.Value.Address.Street);
        Assert.Equal(123, result.Value.Address.PostalCode);
        Assert.Equal(10, result.Value.Address.BuildingNumber);
    }

    [Fact]
    public async Task GetLocation_non_existent_location_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Locations/{Guid.NewGuid()}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<LocationDto?>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task GetLocation_deleted_location_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var deleteResponse = await AppHttpClient.DeleteAsync($"/api/Locations/{location.Id.Value}", cancellationToken);
        Assert.True((await deleteResponse.HandleResponseAsync(cancellationToken)).IsSuccess);

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Locations/{location.Id.Value}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<LocationDto?>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task GetLocation_with_invalid_guid_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Locations/{Guid.Empty}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<LocationDto?>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }
}