using System.Net.Http.Json;
using DirectoryService.Contracts.Positions.Requests;
using DirectoryService.Domain.Positions;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Positions.Commands;

public class CreatePositionTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task CreatePosition_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        string nameValue = "test_name";
        var request = new CreatePositionRequest(nameValue, null, [department.Id.Value]);

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Positions", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert
        Assert.True(createResult.IsSuccess);
        Assert.NotEqual(Guid.Empty, createResult.Value);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .Include(p => p.Departments)
                .FirstOrDefaultAsync(p => p.Id == new PositionId(createResult.Value), cancellationToken);

            Assert.NotNull(position);
            Assert.Equal(nameValue, position.Name.Value);
            Assert.Null(position.Description);
            Assert.Single(position.Departments);
            Assert.Contains(department.Id, position.Departments.Select(dp => dp.DepartmentId));
        });
    }

    [Fact]
    public async Task CreatePosition_with_description_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        string nameValue = "test_name";
        string descriptionValue = "test description";
        var request = new CreatePositionRequest(nameValue, descriptionValue, [department.Id.Value]);

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Positions", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert
        Assert.True(createResult.IsSuccess);
        Assert.NotEqual(Guid.Empty, createResult.Value);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstOrDefaultAsync(p => p.Id == new PositionId(createResult.Value), cancellationToken);

            Assert.NotNull(position);
            Assert.Equal(descriptionValue, position.Description!.Value);
        });
    }

    [Fact]
    public async Task CreatePosition_with_multiple_departments_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department1 = await CreateDepartmentAsync([location.Id.Value], name: "dept_one", identifier: "deptone");
        var department2 = await CreateDepartmentAsync([location.Id.Value], name: "dept_two", identifier: "depttwo");

        var request = new CreatePositionRequest("test_name", null, [department1.Id.Value, department2.Id.Value]);

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Positions", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert
        Assert.True(createResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .Include(p => p.Departments)
                .FirstOrDefaultAsync(p => p.Id == new PositionId(createResult.Value), cancellationToken);

            Assert.NotNull(position);
            Assert.Equal(2, position.Departments.Count);
            Assert.Contains(department1.Id, position.Departments.Select(dp => dp.DepartmentId));
            Assert.Contains(department2.Id, position.Departments.Select(dp => dp.DepartmentId));
        });
    }

    [Fact]
    public async Task CreatePosition_with_empty_name_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        var request = new CreatePositionRequest(string.Empty, null, [department.Id.Value]);

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Positions", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createResult.IsFailure);
        Assert.NotNull(createResult.Error);
        Assert.Contains(createResult.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task CreatePosition_with_invalid_name_length_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        var request = new CreatePositionRequest("sh", null, [department.Id.Value]);

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Positions", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createResult.IsFailure);
        Assert.NotNull(createResult.Error);
        Assert.Contains(createResult.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task CreatePosition_with_empty_department_ids_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new CreatePositionRequest("test_name", null, []);

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Positions", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createResult.IsFailure);
        Assert.NotNull(createResult.Error);
        Assert.Contains(createResult.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            int positionsCount = await dbContext.Positions.CountAsync(cancellationToken);
            Assert.Equal(0, positionsCount);
        });
    }

    [Fact]
    public async Task CreatePosition_with_non_existent_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new CreatePositionRequest("test_name", null, [Guid.NewGuid()]);

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Positions", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createResult.IsFailure);
        Assert.NotNull(createResult.Error);
        Assert.Contains(createResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task CreatePosition_with_inactive_department_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], isActive: false);

        var request = new CreatePositionRequest("test_name", null, [department.Id.Value]);

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Positions", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createResult.IsFailure);
        Assert.NotNull(createResult.Error);
        Assert.Contains(createResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task CreatePosition_without_department_ids_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new CreatePositionRequest("test_name", null, null!);

        // act
        var createResponse = await AppHttpClient.PostAsJsonAsync("/api/Positions", request, cancellationToken);
        var createResult = await createResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createResult.IsFailure);
        Assert.NotNull(createResult.Error);
        Assert.Contains(createResult.Error, e => e.Type == ErrorType.FAILURE);
    }
}