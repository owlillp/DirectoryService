using DirectoryService.Contracts.Positions.Dtos;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace DirectoryService.IntegrationTests.Positions.Queries;

public class GetPositionTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task GetPosition_should_return_position_with_department_ids()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([department.Id.Value], name: "test_pos");

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Positions/{position.Id.Value}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PositionDto>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(position.Id.Value, result.Value.Id);
        Assert.Equal("test_pos", result.Value.Name);
        Assert.Null(result.Value.Description);
        Assert.True(result.Value.IsActive);
        Assert.Single(result.Value.DepartmentIds);
        Assert.Contains(department.Id.Value, result.Value.DepartmentIds);
    }

    [Fact]
    public async Task GetPosition_should_return_position_with_description()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([department.Id.Value], name: "desc_pos");

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Positions/{position.Id.Value}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PositionDto>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(position.Id.Value, result.Value.Id);
    }

    [Fact]
    public async Task GetPosition_should_return_position_with_multiple_department_ids()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department1 = await CreateDepartmentAsync([location.Id.Value], name: "dept_one", identifier: "deptone");
        var department2 = await CreateDepartmentAsync([location.Id.Value], name: "dept_two", identifier: "depttwo");
        var position = await CreatePositionAsync([department1.Id.Value, department2.Id.Value], name: "multi_dept_pos");

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Positions/{position.Id.Value}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PositionDto>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.DepartmentIds.Count);
        Assert.Contains(department1.Id.Value, result.Value.DepartmentIds);
        Assert.Contains(department2.Id.Value, result.Value.DepartmentIds);
    }

    [Fact]
    public async Task GetPosition_non_existent_position_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Positions/{Guid.NewGuid()}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task GetPosition_deleted_position_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([department.Id.Value]);

        // soft delete
        var deleteResponse = await AppHttpClient.DeleteAsync($"/api/Positions/{position.Id.Value}", cancellationToken);
        Assert.True((await deleteResponse.HandleResponseAsync(cancellationToken)).IsSuccess);

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Positions/{position.Id.Value}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task GetPosition_with_invalid_guid_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Positions/{Guid.Empty}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }
}