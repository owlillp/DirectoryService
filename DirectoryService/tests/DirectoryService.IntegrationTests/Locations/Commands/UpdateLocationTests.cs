using System.Net.Http.Json;
using DirectoryService.Contracts.Locations.Dtos;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Locations.Commands;

public class UpdateLocationTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task UpdateLocation_name_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync(name: "old_name");

        string newNameValue = "new_name";
        var request = new UpdateLocationRequest { Name = newNameValue };

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Locations/{location.Id.Value}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var updatedLocation = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == location.Id, cancellationToken);

            Assert.NotNull(updatedLocation);
            Assert.Equal(newNameValue, updatedLocation.Name.Value);
        });
    }

    [Fact]
    public async Task UpdateLocation_address_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var newAddress = new LocationAddressDto
        {
            Country = "new_country",
            City = "new_city",
            Street = "new_street",
            PostalCode = 999,
            BuildingNumber = 5,
            Apartment = "10",
        };

        var request = new UpdateLocationRequest { Address = newAddress };

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Locations/{location.Id.Value}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var updatedLocation = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == location.Id, cancellationToken);

            Assert.NotNull(updatedLocation);
            Assert.Equal(newAddress.Country, updatedLocation.Address.Country);
            Assert.Equal(newAddress.City, updatedLocation.Address.City);
            Assert.Equal(newAddress.Street, updatedLocation.Address.Street);
            Assert.Equal(newAddress.PostalCode, updatedLocation.Address.PostalCode);
            Assert.Equal(newAddress.BuildingNumber, updatedLocation.Address.BuildingNumber);
            Assert.Equal(newAddress.Apartment, updatedLocation.Address.Apartment);
        });
    }

    [Fact]
    public async Task UpdateLocation_timezone_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync(timeZone: "Europe/Moscow");

        string newTimeZoneValue = "Asia/Yekaterinburg";
        var request = new UpdateLocationRequest { TimeZone = newTimeZoneValue };

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Locations/{location.Id.Value}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var updatedLocation = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == location.Id, cancellationToken);

            Assert.NotNull(updatedLocation);
            Assert.Equal(newTimeZoneValue, updatedLocation.Timezone.Value);
        });
    }

    [Fact]
    public async Task UpdateLocation_all_fields_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync(name: "old_name", timeZone: "Europe/Moscow");

        var newAddress = new LocationAddressDto
        {
            Country = "new_country",
            City = "new_city",
            Street = "new_street",
            PostalCode = 888,
            BuildingNumber = 3,
        };

        var request = new UpdateLocationRequest
        {
            Name = "new_name",
            Address = newAddress,
            TimeZone = "Asia/Yekaterinburg",
        };

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Locations/{location.Id.Value}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var updatedLocation = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == location.Id, cancellationToken);

            Assert.NotNull(updatedLocation);
            Assert.Equal("new_name", updatedLocation.Name.Value);
            Assert.Equal(newAddress.Country, updatedLocation.Address.Country);
            Assert.Equal(newAddress.City, updatedLocation.Address.City);
            Assert.Equal("Asia/Yekaterinburg", updatedLocation.Timezone.Value);
        });
    }

    [Fact]
    public async Task UpdateLocation_with_empty_name_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var request = new UpdateLocationRequest { Name = string.Empty };

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Locations/{location.Id.Value}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            var locationAfterAttempt = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == location.Id, cancellationToken);

            Assert.NotNull(locationAfterAttempt);
            Assert.Equal(location.Name.Value, locationAfterAttempt.Name.Value);
        });
    }

    [Fact]
    public async Task UpdateLocation_with_invalid_name_length_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var request = new UpdateLocationRequest { Name = "sh" };

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Locations/{location.Id.Value}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task UpdateLocation_non_existent_location_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new UpdateLocationRequest { Name = "new_name" };

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Locations/{Guid.NewGuid()}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UpdateLocation_deleted_location_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var deleteResponse = await AppHttpClient.DeleteAsync($"/api/Locations/{location.Id.Value}", cancellationToken);
        var deleteResult = await deleteResponse.HandleResponseAsync(cancellationToken);
        Assert.True(deleteResult.IsSuccess);

        var request = new UpdateLocationRequest { Name = "new_name" };

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Locations/{location.Id.Value}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UpdateLocation_with_invalid_guid_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new UpdateLocationRequest { Name = "new_name" };

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Locations/{Guid.Empty}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.VALIDATION);
    }
}