using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Locations.Queries;

public class GetTopByPositionsTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task GetTopByPositions_should_return_top_locations_by_departments_count()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location1 = await CreateLocationAsync(name: "top1", city: "city1");
        var location2 = await CreateLocationAsync(name: "top2", city: "city2");
        await CreateLocationAsync(name: "top3", city: "city3");

        var dept1A = await CreateDepartmentAsync([location1.Id.Value], name: "dept1a", identifier: "deptonea");
        var dept1B = await CreateDepartmentAsync([location1.Id.Value], name: "dept1b", identifier: "deptoneb");
        var dept1C = await CreateDepartmentAsync([location1.Id.Value], name: "dept1c", identifier: "deptonec");
        var dept2 = await CreateDepartmentAsync([location2.Id.Value], name: "dept2", identifier: "depttwo");

        await CreatePositionAsync([dept1A.Id.Value], name: "pos1a");
        await CreatePositionAsync([dept1B.Id.Value], name: "pos1b");
        await CreatePositionAsync([dept1C.Id.Value], name: "pos1c");
        await CreatePositionAsync([dept2.Id.Value], name: "pos2");

        // act
        var httpResponse = await AppHttpClient.GetAsync("/api/Locations/top?topCount=2", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<GetTopLocationsResponse>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(2, result.Value.Locations.Count);
        Assert.Equal(location1.Id.Value, result.Value.Locations[0].Id);
        Assert.Equal(3, result.Value.Locations[0].DepartmentsCount);
        Assert.Equal(location2.Id.Value, result.Value.Locations[1].Id);
        Assert.Equal(1, result.Value.Locations[1].DepartmentsCount);
    }

    [Fact]
    public async Task GetTopByPositions_with_top_count_zero_should_fail_validation()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync(name: "loc", city: "city");
        var department = await CreateDepartmentAsync([location.Id.Value], name: "dept", identifier: "dept");
        await CreatePositionAsync([department.Id.Value]);

        // act
        var httpResponse = await AppHttpClient.GetAsync("/api/Locations/top?topCount=0", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<GetTopLocationsResponse?>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetTopByPositions_should_exclude_locations_without_departments()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        await CreateLocationAsync(name: "orphan", city: "orphan_city"); // no department

        var locationWithDept = await CreateLocationAsync(name: "with_dept", city: "dept_city");
        var department = await CreateDepartmentAsync([locationWithDept.Id.Value], name: "dept", identifier: "dept");
        await CreatePositionAsync([department.Id.Value]);

        // act
        var httpResponse = await AppHttpClient.GetAsync("/api/Locations/top?topCount=10", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<GetTopLocationsResponse>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Locations);
        Assert.Equal(locationWithDept.Id.Value, result.Value.Locations[0].Id);
    }

    [Fact]
    public async Task GetTopByPositions_should_order_by_descending_departments_count()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var locationLow = await CreateLocationAsync(name: "low", city: "low_city");
        var locationHigh = await CreateLocationAsync(name: "high", city: "high_city");

        var deptLow = await CreateDepartmentAsync([locationLow.Id.Value], name: "dept_low", identifier: "deptlow");
        var deptHigh = await CreateDepartmentAsync([locationHigh.Id.Value], name: "dept_high", identifier: "depthigh");

        await CreatePositionAsync([deptLow.Id.Value], name: "pos_low");
        for (int i = 0; i < 5; i++)
        {
            await CreatePositionAsync([deptHigh.Id.Value], name: $"pos_high_{i}");
        }

        // act
        var httpResponse = await AppHttpClient.GetAsync("/api/Locations/top?topCount=10", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<GetTopLocationsResponse>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Locations.Count);
        Assert.Equal("high", result.Value.Locations[0].Name);
        Assert.Equal("low", result.Value.Locations[1].Name);
        Assert.Equal(1, result.Value.Locations[0].DepartmentsCount);
        Assert.Equal(1, result.Value.Locations[1].DepartmentsCount);
    }
}