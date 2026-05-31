using System.Net.Http.Json;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Departments.Commands;

public class UpdateDepartmentTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task UpdateDepartment_name_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "old_name", identifier: "dept");

        string newNameValue = "new_name";
        var request = new UpdateDepartmentRequest(newNameValue);

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync(
            $"/api/Departments/{department.Id.Value}",
            request,
            cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var updatedDepartment = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department.Id, cancellationToken);

            Assert.NotNull(updatedDepartment);
            Assert.Equal(newNameValue, updatedDepartment.Name.Value);
        });
    }

    [Fact]
    public async Task UpdateDepartment_with_same_name_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "same_name", identifier: "dept");

        var request = new UpdateDepartmentRequest("same_name");

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync(
            $"/api/Departments/{department.Id.Value}",
            request,
            cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var updatedDepartment = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department.Id, cancellationToken);

            Assert.NotNull(updatedDepartment);
            Assert.Equal("same_name", updatedDepartment.Name.Value);
        });
    }

    [Fact]
    public async Task UpdateDepartment_with_empty_name_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "old_name", identifier: "dept");

        var request = new UpdateDepartmentRequest(string.Empty);

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync(
            $"/api/Departments/{department.Id.Value}",
            request,
            cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            var departmentAfterAttempt = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department.Id, cancellationToken);

            Assert.NotNull(departmentAfterAttempt);
            Assert.Equal("old_name", departmentAfterAttempt.Name.Value);
        });
    }

    [Fact]
    public async Task UpdateDepartment_with_invalid_name_length_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "old_name", identifier: "dept");

        var request = new UpdateDepartmentRequest("sh");

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync(
            $"/api/Departments/{department.Id.Value}",
            request,
            cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task UpdateDepartment_non_existent_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new UpdateDepartmentRequest("new_name");

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync(
            $"/api/Departments/{Guid.NewGuid()}",
            request,
            cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UpdateDepartment_deleted_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "old_name", identifier: "dept");

        var deleteResponse = await AppHttpClient.DeleteAsync($"/api/Departments/{department.Id.Value}", cancellationToken);
        var deleteResult = await deleteResponse.HandleResponseAsync(cancellationToken);
        Assert.True(deleteResult.IsSuccess);

        var request = new UpdateDepartmentRequest("new_name");

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync(
            $"/api/Departments/{department.Id.Value}",
            request,
            cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UpdateDepartment_with_invalid_guid_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new UpdateDepartmentRequest("new_name");

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync(
            $"/api/Departments/{Guid.Empty}",
            request,
            cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.VALIDATION);
    }
}
