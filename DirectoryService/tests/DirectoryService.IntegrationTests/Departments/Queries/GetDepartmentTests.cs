using DirectoryService.Contracts.Departments.Dtos;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Departments.Queries;

public class GetDepartmentTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task GetDepartment_should_return_department_with_location_and_position_ids()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "test_dept", identifier: "test");
        var position = await CreatePositionAsync([department.Id.Value], name: "test_pos");

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Departments/{department.Id.Value}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<DepartmentDto>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(department.Id.Value, result.Value.Id);
        Assert.Equal("test_dept", result.Value.Name);
        Assert.Equal("test", result.Value.Identifier);
        Assert.True(result.Value.IsActive);
        Assert.Single(result.Value.LocationIds);
        Assert.Contains(location.Id.Value, result.Value.LocationIds);
        Assert.Single(result.Value.PositionIds);
        Assert.Contains(position.Id.Value, result.Value.PositionIds);
    }

    [Fact]
    public async Task GetDepartment_should_return_correct_parent_id_and_depth()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var parent = await CreateDepartmentAsync([location.Id.Value], name: "parent", identifier: "parent");
        var child = await CreateDepartmentAsync(
            [location.Id.Value],
            name: "child",
            identifier: "child",
            parent: parent);

        // act
        var parentResponse = await AppHttpClient.GetAsync($"/api/Departments/{parent.Id.Value}", cancellationToken);
        var parentResult = await parentResponse.HandleResponseAsync<DepartmentDto>(cancellationToken);

        var childResponse = await AppHttpClient.GetAsync($"/api/Departments/{child.Id.Value}", cancellationToken);
        var childResult = await childResponse.HandleResponseAsync<DepartmentDto>(cancellationToken);

        // assert
        Assert.True(parentResult.IsSuccess);
        Assert.NotNull(parentResult.Value);
        Assert.Null(parentResult.Value.ParentId);
        Assert.Equal(0, parentResult.Value.Depth);

        Assert.True(childResult.IsSuccess);
        Assert.NotNull(childResult.Value);
        Assert.NotNull(childResult.Value.ParentId);
        Assert.Equal(parent.Id.Value, childResult.Value.ParentId);
        Assert.Equal(1, childResult.Value.Depth);
    }

    [Fact]
    public async Task GetDepartment_non_existent_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Departments/{Guid.NewGuid()}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<DepartmentDto?>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task GetDepartment_deleted_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "dept", identifier: "dept");

        var deleteResponse = await AppHttpClient.DeleteAsync($"/api/Departments/{department.Id.Value}", cancellationToken);
        Assert.True((await deleteResponse.HandleResponseAsync(cancellationToken)).IsSuccess);

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Departments/{department.Id.Value}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<DepartmentDto?>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetDepartment_with_invalid_guid_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var httpResponse = await AppHttpClient.GetAsync($"/api/Departments/{Guid.Empty}", cancellationToken);
        var result = await httpResponse.HandleResponseAsync<DepartmentDto?>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }
}
