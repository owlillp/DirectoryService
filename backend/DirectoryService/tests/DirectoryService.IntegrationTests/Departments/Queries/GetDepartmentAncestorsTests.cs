using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace DirectoryService.IntegrationTests.Departments.Queries;

public class GetDepartmentAncestorsTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task GetDepartmentAncestors_should_return_all_ancestors_ordered_by_depth()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var grandParent = await CreateDepartmentAsync([location.Id.Value], name: "grandparent", identifier: "grandparent");
        var parent = await CreateDepartmentAsync([location.Id.Value], name: "parent", identifier: "parent", parent: grandParent);
        var child = await CreateDepartmentAsync([location.Id.Value], name: "child", identifier: "child", parent: parent);

        // act
        var httpResponse = await AppHttpClient.GetAsync(
            $"/api/Departments/{child.Id.Value}/ancestors",
            cancellationToken);
        var result = await httpResponse.HandleResponseAsync<GetDepartmentAncestorsResponse>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(child.Id.Value, result.Value.TargetDepartmentId);
        Assert.Equal(2, result.Value.AncestorDepartments.Count);

        Assert.Equal(grandParent.Id.Value, result.Value.AncestorDepartments[0].Id);
        Assert.Equal(0, result.Value.AncestorDepartments[0].Depth);
        Assert.Equal(parent.Id.Value, result.Value.AncestorDepartments[1].Id);
        Assert.Equal(1, result.Value.AncestorDepartments[1].Depth);
    }

    [Fact]
    public async Task GetDepartmentAncestors_root_department_should_return_empty()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var root = await CreateDepartmentAsync([location.Id.Value], name: "root", identifier: "root");

        // act
        var httpResponse = await AppHttpClient.GetAsync(
            $"/api/Departments/{root.Id.Value}/ancestors",
            cancellationToken);
        var result = await httpResponse.HandleResponseAsync<GetDepartmentAncestorsResponse>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(root.Id.Value, result.Value.TargetDepartmentId);
        Assert.Empty(result.Value.AncestorDepartments);
    }

    [Fact]
    public async Task GetDepartmentAncestors_with_deep_hierarchy_should_return_all()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var dept1 = await CreateDepartmentAsync([location.Id.Value], name: "dept1", identifier: "dept_one");
        var dept2 = await CreateDepartmentAsync([location.Id.Value], name: "dept2", identifier: "dept_two", parent: dept1);
        var dept3 = await CreateDepartmentAsync([location.Id.Value], name: "dept3", identifier: "dept_three", parent: dept2);
        var dept4 = await CreateDepartmentAsync([location.Id.Value], name: "dept4", identifier: "dept_four", parent: dept3);

        // act
        var httpResponse = await AppHttpClient.GetAsync(
            $"/api/Departments/{dept4.Id.Value}/ancestors",
            cancellationToken);
        var result = await httpResponse.HandleResponseAsync<GetDepartmentAncestorsResponse>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.AncestorDepartments.Count);
        Assert.Equal(dept1.Id.Value, result.Value.AncestorDepartments[0].Id);
        Assert.Equal(dept2.Id.Value, result.Value.AncestorDepartments[1].Id);
        Assert.Equal(dept3.Id.Value, result.Value.AncestorDepartments[2].Id);
    }

    [Fact]
    public async Task GetDepartmentAncestors_non_existent_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var httpResponse = await AppHttpClient.GetAsync(
            $"/api/Departments/{Guid.NewGuid()}/ancestors",
            cancellationToken);
        var result = await httpResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task GetDepartmentAncestors_with_invalid_guid_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var httpResponse = await AppHttpClient.GetAsync(
            $"/api/Departments/{Guid.Empty}/ancestors",
            cancellationToken);
        var result = await httpResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }
}
