using System.Net.Http.Json;
using DirectoryService.Contracts.Locations.Dtos;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace DirectoryService.IntegrationTests.Locations.Commands;

public class CreateLocationTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task CreateLocation_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        string nameValue = "test_location";
        string timeZoneValue = "Europe/Moscow";
        var address = new LocationAddressDto
        {
            Country = "test_country",
            City = "test_city",
            Street = "test_street",
            PostalCode = 101,
            BuildingNumber = 1,
        };

        var request = new CreateLocationRequest(nameValue, address, timeZoneValue);

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Locations", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert
        Assert.True(createResult.IsSuccess);
        Assert.NotEqual(Guid.Empty, createResult.Value);

        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == new LocationId(createResult.Value), cancellationToken);

            Assert.NotNull(location);
            Assert.Equal(nameValue, location.Name.Value);
            Assert.Equal(timeZoneValue, location.Timezone.Value);
            Assert.Equal(address.Country, location.Address.Country);
            Assert.Equal(address.City, location.Address.City);
            Assert.Equal(address.Street, location.Address.Street);
            Assert.Equal(address.PostalCode, location.Address.PostalCode);
            Assert.Equal(address.BuildingNumber, location.Address.BuildingNumber);
            Assert.True(location.IsActive);
        });
    }

    [Fact]
    public async Task CreateLocation_with_apartment_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var address = new LocationAddressDto
        {
            Country = "test_country",
            City = "test_city",
            Street = "test_street",
            PostalCode = 101,
            BuildingNumber = 1,
            Apartment = "42",
        };

        var request = new CreateLocationRequest("test_location", address, "Europe/Moscow");

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Locations", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert
        Assert.True(createResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == new LocationId(createResult.Value), cancellationToken);

            Assert.NotNull(location);
            Assert.Equal("42", location.Address.Apartment);
        });
    }

    [Fact]
    public async Task CreateLocation_with_empty_name_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var address = new LocationAddressDto
        {
            Country = "test_country",
            City = "test_city",
            Street = "test_street",
            PostalCode = 101,
            BuildingNumber = 1,
        };

        var request = new CreateLocationRequest(string.Empty, address, "Europe/Moscow");

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Locations", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(createResult.IsFailure);
        Assert.NotNull(createResult.Error);
        Assert.Contains(createResult.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task CreateLocation_with_invalid_name_length_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var address = new LocationAddressDto
        {
            Country = "test_country",
            City = "test_city",
            Street = "test_street",
            PostalCode = 101,
            BuildingNumber = 1,
        };

        var request = new CreateLocationRequest("sh", address, "Europe/Moscow");

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Locations", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(createResult.IsFailure);
        Assert.NotNull(createResult.Error);
        Assert.Contains(createResult.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            int locationsCount = await dbContext.Locations.CountAsync(cancellationToken);
            Assert.Equal(0, locationsCount);
        });
    }

    [Fact]
    public async Task CreateLocation_with_empty_city_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var address = new LocationAddressDto
        {
            Country = "test_country",
            City = string.Empty,
            Street = "test_street",
            PostalCode = 101,
            BuildingNumber = 1,
        };

        var request = new CreateLocationRequest("test_location", address, "Europe/Moscow");

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Locations", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(createResult.IsFailure);
        Assert.NotNull(createResult.Error);
        Assert.Contains(createResult.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task CreateLocation_with_invalid_timezone_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var address = new LocationAddressDto
        {
            Country = "test_country",
            City = "test_city",
            Street = "test_street",
            PostalCode = 101,
            BuildingNumber = 1,
        };

        var request = new CreateLocationRequest("test_location", address, "invalid_timezone");

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Locations", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(createResult.IsFailure);
        Assert.NotNull(createResult.Error);
        Assert.Contains(createResult.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task CreateLocation_with_invalid_postal_code_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var address = new LocationAddressDto
        {
            Country = "test_country",
            City = "test_city",
            Street = "test_street",
            PostalCode = -1,
            BuildingNumber = 1,
        };

        var request = new CreateLocationRequest("test_location", address, "Europe/Moscow");

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Locations", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(createResult.IsFailure);
        Assert.NotNull(createResult.Error);
        Assert.Contains(createResult.Error, e => e.Type == ErrorType.VALIDATION);
    }
}