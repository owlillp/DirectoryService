using System.Net.Http.Json;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Departments.Commands;

public class UpdateDepartmentParentTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task UpdateDepartmentParent_without_child_to_parent_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var parentDepartment = await CreateDepartmentAsync([location.Id.Value], "parent_department", "parent");
        var departmentToMove = await CreateDepartmentAsync([location.Id.Value], "move_department", "move");

        var request = new UpdateDepartmentParentRequest(parentDepartment.Id.Value);

        const int targetMovedDepth = 1;
        const int targetParentDepth = 0;

        // act
        var updateDepartmentParentResponse = await AppHttpClient.PutAsJsonAsync($"/api/Departments/{departmentToMove.Id.Value}/parent", request, cancellationToken);
        var updateDepartmentParentResult = await updateDepartmentParentResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateDepartmentParentResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var targetMovedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentToMove.Id, cancellationToken);

            var targetParentDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == parentDepartment.Id, cancellationToken);

            Assert.Equal(targetMovedDepth, targetMovedDepartment.Depth);
            Assert.Equal(targetParentDepth, targetParentDepartment.Depth);

            var targetChildPath = targetParentDepartment.Path.CreateChild(targetMovedDepartment.Identifier);
            var targetParentPath = DepartmentPath.CreateParent(targetParentDepartment.Identifier);

            Assert.Equal(targetChildPath, targetMovedDepartment.Path);
            Assert.Equal(targetParentPath, targetParentDepartment.Path);
        });
    }

    [Fact]
    public async Task UpdateDepartmentParent_without_child_to_root_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var parentDepartment = await CreateDepartmentAsync([location.Id.Value], "parent_department", "parent");
        var departmentToMove = await CreateDepartmentAsync([location.Id.Value], "move_department", "move", parent: parentDepartment);

        var request = new UpdateDepartmentParentRequest(null);

        const int targetMovedDepth = 0;

        // act
        var updateDepartmentParentResponse = await AppHttpClient.PutAsJsonAsync($"/api/Departments/{departmentToMove.Id.Value}/parent", request, cancellationToken);
        var updateDepartmentParentResult = await updateDepartmentParentResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateDepartmentParentResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var targetMovedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentToMove.Id, cancellationToken);

            Assert.Equal(targetMovedDepth, targetMovedDepartment.Depth);

            var targetPath = DepartmentPath.CreateParent(targetMovedDepartment.Identifier);

            Assert.Equal(targetPath, targetMovedDepartment.Path);
        });
    }

    [Fact]
    public async Task UpdateDepartmentParent_with_child_to_parent_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var parentDepartment = await CreateDepartmentAsync([location.Id.Value], "parent_department", "parent");
        var departmentToMove = await CreateDepartmentAsync([location.Id.Value], "move_department", "move");
        var childDepartment = await CreateDepartmentAsync([location.Id.Value], "child_department", "child", parent: departmentToMove);
        var subChildDepartment = await CreateDepartmentAsync([location.Id.Value], "sub_child_department", "subchild", parent: childDepartment);

        var request = new UpdateDepartmentParentRequest(parentDepartment.Id.Value);

        const int targetParentDepth = 0;
        const int targetMovedDepth = 1;
        const int targetChildDepth = 2;
        const int targetSubChildDepth = 3;

        // act
        var updateDepartmentParentResponse = await AppHttpClient.PutAsJsonAsync($"/api/Departments/{departmentToMove.Id.Value}/parent", request, cancellationToken);
        var updateDepartmentParentResult = await updateDepartmentParentResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateDepartmentParentResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var targetMovedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentToMove.Id, cancellationToken);

            var targetParentDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == parentDepartment.Id, cancellationToken);

            var targetChildDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == childDepartment.Id, cancellationToken);

            var targetSubChildDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == subChildDepartment.Id, cancellationToken);

            Assert.Equal(targetMovedDepth, targetMovedDepartment.Depth);
            Assert.Equal(targetParentDepth, targetParentDepartment.Depth);
            Assert.Equal(targetChildDepth, targetChildDepartment.Depth);
            Assert.Equal(targetSubChildDepth, targetSubChildDepartment.Depth);

            var targetParentPath = DepartmentPath.CreateParent(targetParentDepartment.Identifier);
            var targetMovedPath = targetParentDepartment.Path.CreateChild(targetMovedDepartment.Identifier);
            var targetChildPath = targetMovedDepartment.Path.CreateChild(targetChildDepartment.Identifier);
            var targetSubChildPath = targetChildDepartment.Path.CreateChild(targetSubChildDepartment.Identifier);

            Assert.Equal(targetParentPath, targetParentDepartment.Path);
            Assert.Equal(targetMovedPath, targetMovedDepartment.Path);
            Assert.Equal(targetChildPath, targetChildDepartment.Path);
            Assert.Equal(targetSubChildPath, targetSubChildDepartment.Path);
        });
    }

    [Fact]
    public async Task UpdateDepartmentParent_with_child_to_root_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var parentDepartment = await CreateDepartmentAsync([location.Id.Value], "parent_department", "parent");
        var departmentToMove = await CreateDepartmentAsync([location.Id.Value], "move_department", "move", parent: parentDepartment);
        var childDepartment = await CreateDepartmentAsync([location.Id.Value], "child_department", "child", parent: departmentToMove);
        var subChildDepartment = await CreateDepartmentAsync([location.Id.Value], "sub_child_department", "subchild", parent: childDepartment);

        var request = new UpdateDepartmentParentRequest(null);

        const int targetMovedDepth = 0;
        const int targetChildDepth = 1;
        const int targetSubChildDepth = 2;

        // act
        var updateDepartmentParentResponse = await AppHttpClient.PutAsJsonAsync($"/api/Departments/{departmentToMove.Id.Value}/parent", request, cancellationToken);
        var updateDepartmentParentResult = await updateDepartmentParentResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateDepartmentParentResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var targetMovedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentToMove.Id, cancellationToken);

            var targetChildDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == childDepartment.Id, cancellationToken);

            var targetSubChildDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == subChildDepartment.Id, cancellationToken);

            Assert.Equal(targetMovedDepth, targetMovedDepartment.Depth);
            Assert.Equal(targetChildDepth, targetChildDepartment.Depth);
            Assert.Equal(targetSubChildDepth, targetSubChildDepartment.Depth);

            var targetMovedPath = DepartmentPath.CreateParent(targetMovedDepartment.Identifier);
            var targetChildPath = targetMovedDepartment.Path.CreateChild(targetChildDepartment.Identifier);
            var targetSubChildPath = targetChildDepartment.Path.CreateChild(targetSubChildDepartment.Identifier);

            Assert.Equal(targetMovedPath, targetMovedDepartment.Path);
            Assert.Equal(targetChildPath, targetChildDepartment.Path);
            Assert.Equal(targetSubChildPath, targetSubChildDepartment.Path);
        });
    }

    [Fact]
    public async Task UpdateDepartmentParent_with_cyclic_hierarchy_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var departmentToMove = await CreateDepartmentAsync([location.Id.Value], "move_department", "move");
        var childDepartment = await CreateDepartmentAsync([location.Id.Value], "child_department", "child", parent: departmentToMove);
        var subChildDepartment = await CreateDepartmentAsync([location.Id.Value], "sub_child_department", "subchild", parent: childDepartment);

        var request = new UpdateDepartmentParentRequest(subChildDepartment.Id.Value);

        // act
        var updateDepartmentParentResponse = await AppHttpClient.PutAsJsonAsync($"/api/Departments/{departmentToMove.Id.Value}/parent", request, cancellationToken);
        var updateDepartmentParentResult = await updateDepartmentParentResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateDepartmentParentResult.IsFailure);
        Assert.NotNull(updateDepartmentParentResult.Error);
        Assert.Contains(updateDepartmentParentResult.Error, e => e.Type == ErrorType.CONFLICT);
    }

    [Fact]
    public async Task UpdateDepartmentParent_self_repeat__should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var departmentToMove = await CreateDepartmentAsync([location.Id.Value], "move_department", "move");

        var request = new UpdateDepartmentParentRequest(departmentToMove.Id.Value);

        // act
        var updateDepartmentParentResponse = await AppHttpClient.PutAsJsonAsync($"/api/Departments/{departmentToMove.Id.Value}/parent", request, cancellationToken);
        var updateDepartmentParentResult = await updateDepartmentParentResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateDepartmentParentResult.IsFailure);
        Assert.NotNull(updateDepartmentParentResult.Error);
        Assert.Contains(updateDepartmentParentResult.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task UpdateDepartmentParent_to_empty_parent_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var departmentToMove = await CreateDepartmentAsync([location.Id.Value], "move_department", "move");

        var request = new UpdateDepartmentParentRequest(Guid.Empty);

        // act
        var updateDepartmentParentResponse = await AppHttpClient.PutAsJsonAsync($"/api/Departments/{departmentToMove.Id.Value}/parent", request, cancellationToken);
        var updateDepartmentParentResult = await updateDepartmentParentResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateDepartmentParentResult.IsFailure);
        Assert.NotNull(updateDepartmentParentResult.Error);
        Assert.Contains(updateDepartmentParentResult.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task UpdateDepartmentParent_to_non_existent_parent_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var departmentToMove = await CreateDepartmentAsync([location.Id.Value], "move_department", "move");

        var request = new UpdateDepartmentParentRequest(Guid.NewGuid());

        // act
        var updateDepartmentParentResponse = await AppHttpClient.PutAsJsonAsync($"/api/Departments/{departmentToMove.Id.Value}/parent", request, cancellationToken);
        var updateDepartmentParentResult = await updateDepartmentParentResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateDepartmentParentResult.IsFailure);
        Assert.NotNull(updateDepartmentParentResult.Error);
        Assert.Contains(updateDepartmentParentResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UpdateDepartmentParent_to_inactive_parent_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var parentDepartment = await CreateDepartmentAsync([location.Id.Value], "parent_department", "parent", isActive: false);
        var departmentToMove = await CreateDepartmentAsync([location.Id.Value], "move_department", "move");

        var request = new UpdateDepartmentParentRequest(parentDepartment.Id.Value);

        // act
        var updateDepartmentParentResponse = await AppHttpClient.PutAsJsonAsync($"/api/Departments/{departmentToMove.Id.Value}/parent", request, cancellationToken);
        var updateDepartmentParentResult = await updateDepartmentParentResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateDepartmentParentResult.IsFailure);
        Assert.NotNull(updateDepartmentParentResult.Error);
        Assert.Contains(updateDepartmentParentResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UpdateDepartmentParent_with_non_existent_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var parentDepartment = await CreateDepartmentAsync([location.Id.Value], "parent_department", "parent");

        var request = new UpdateDepartmentParentRequest(parentDepartment.Id.Value);

        // act
        var updateDepartmentParentResponse = await AppHttpClient.PutAsJsonAsync($"/api/Departments/{Guid.NewGuid()}/parent", request, cancellationToken);
        var updateDepartmentParentResult = await updateDepartmentParentResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateDepartmentParentResult.IsFailure);
        Assert.NotNull(updateDepartmentParentResult.Error);
        Assert.Contains(updateDepartmentParentResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UpdateDepartmentParent_with_inactive_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var parentDepartment = await CreateDepartmentAsync([location.Id.Value], "parent_department", "parent");
        var departmentToMove = await CreateDepartmentAsync([location.Id.Value], "move_department", "move", isActive: false);

        var request = new UpdateDepartmentParentRequest(parentDepartment.Id.Value);

        // act
        var updateDepartmentParentResponse = await AppHttpClient.PutAsJsonAsync($"/api/Departments/{departmentToMove.Id.Value}/parent", request, cancellationToken);
        var updateDepartmentParentResult = await updateDepartmentParentResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateDepartmentParentResult.IsFailure);
        Assert.NotNull(updateDepartmentParentResult.Error);
        Assert.Contains(updateDepartmentParentResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }
}