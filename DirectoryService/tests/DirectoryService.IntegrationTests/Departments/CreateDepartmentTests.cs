using System.Net.Http.Json;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task CreateDepartment_root_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        string nameValue = "test_name";
        string identifierValue = "test";

        var request = new CreateDepartmentRequest(
            nameValue,
            identifierValue,
            null,
            [location.Id.Value]);

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsSuccess);
        Assert.NotEqual(Guid.Empty, createDepartmentResult.Value);

        await ExecuteInDb(async dbContext =>
        {
            var targetDepartmentId = new DepartmentId(createDepartmentResult.Value);

            var department = await dbContext.Departments
                .Include(d => d.Locations)
                .FirstOrDefaultAsync(
                    d => d.Id == targetDepartmentId,
                    cancellationToken: cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(department.Id, targetDepartmentId);
            Assert.Null(department.ParentId);
            Assert.Equal(department.Name.Value, nameValue);
            Assert.Equal(department.Identifier.Value, identifierValue);
            Assert.Single(department.Locations);
            Assert.Contains(location.Id, department.Locations.Select(l => l.LocationId));
        });
    }

    [Fact]
    public async Task CreateDepartment_child_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        string parentNameValue = "parent_name";
        string parentIdentifierValue = "parent";

        var parentDepartment = await CreateDepartmentAsync([location.Id.Value], parentNameValue, parentIdentifierValue);

        string childNameValue = "child_name";
        string childIdentifierValue = "child";

        var request = new CreateDepartmentRequest(
            childNameValue,
            childIdentifierValue,
            parentDepartment.Id.Value,
            [location.Id.Value]);

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsSuccess);
        Assert.NotEqual(Guid.Empty, createDepartmentResult.Value);

        await ExecuteInDb(async dbContext =>
        {
            var targetDepartmentId = new DepartmentId(createDepartmentResult.Value);

            var department = await dbContext.Departments
                .Include(d => d.Locations)
                .FirstOrDefaultAsync(
                    d => d.Id == targetDepartmentId,
                    cancellationToken: cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(department.Id, targetDepartmentId);
            Assert.NotNull(department.ParentId);
            Assert.Equal(department.ParentId, parentDepartment.Id);
            Assert.Equal(department.Name.Value, childNameValue);
            Assert.Equal(department.Identifier.Value, childIdentifierValue);
            Assert.Single(department.Locations);
            Assert.Contains(location.Id, department.Locations.Select(l => l.LocationId));
        });
    }

    [Fact]
    public async Task CreateDepartment_root_with_several_locations_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        const int locationsLength = 3;
        var locations = new List<Location>();

        for (int i = 0; i < locationsLength; i++)
        {
            var location = await CreateLocationAsync($"name_{i}", $"country_{i}");
            locations.Add(location);
        }

        string nameValue = "test_name";
        string identifierValue = "test";

        var request = new CreateDepartmentRequest(
            nameValue,
            identifierValue,
            null,
            locations.Select(l => l.Id.Value));

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsSuccess);
        Assert.NotEqual(Guid.Empty, createDepartmentResult.Value);

        await ExecuteInDb(async dbContext =>
        {
            var targetDepartmentId = new DepartmentId(createDepartmentResult.Value);

            var department = await dbContext.Departments
                .Include(d => d.Locations)
                .FirstOrDefaultAsync(
                    d => d.Id == targetDepartmentId,
                    cancellationToken: cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(department.Id, targetDepartmentId);
            Assert.Null(department.ParentId);
            Assert.Equal(department.Name.Value, nameValue);
            Assert.Equal(department.Identifier.Value, identifierValue);
            Assert.Equal(locationsLength, department.Locations.Count);
            Assert.All(department.Locations, dl => Assert.Contains(dl.LocationId, locations.Select(l => l.Id)));
        });
    }

    [Fact]
    public async Task CreateDepartment_child_with_several_locations_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        const int locationsLength = 3;
        var locations = new List<Location>();

        for (int i = 0; i < locationsLength; i++)
        {
            var location = await CreateLocationAsync($"name_{i}", $"country_{i}");
            locations.Add(location);
        }

        string parentNameValue = "parent_name";
        string parentIdentifierValue = "parent";

        var parentDepartment = await CreateDepartmentAsync(locations.Select(l => l.Id.Value), parentNameValue, parentIdentifierValue);

        string childNameValue = "child_name";
        string childIdentifierValue = "child";

        var request = new CreateDepartmentRequest(
            childNameValue,
            childIdentifierValue,
            parentDepartment.Id.Value,
            locations.Select(l => l.Id.Value));

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsSuccess);
        Assert.NotEqual(Guid.Empty, createDepartmentResult.Value);

        await ExecuteInDb(async dbContext =>
        {
            var targetDepartmentId = new DepartmentId(createDepartmentResult.Value);

            var department = await dbContext.Departments
                .Include(d => d.Locations)
                .FirstOrDefaultAsync(
                    d => d.Id == targetDepartmentId,
                    cancellationToken: cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(department.Id, targetDepartmentId);
            Assert.NotNull(department.ParentId);
            Assert.Equal(department.ParentId, parentDepartment.Id);
            Assert.Equal(department.Name.Value, childNameValue);
            Assert.Equal(department.Identifier.Value, childIdentifierValue);
            Assert.Equal(locationsLength, department.Locations.Count);
            Assert.All(department.Locations, dl => Assert.Contains(dl.LocationId, locations.Select(l => l.Id)));
        });
    }

    [Fact]
    public async Task CreateDepartment_with_null_locations_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        string nameValue = "test_name";
        string identifierValue = "test";

        var request = new CreateDepartmentRequest(
            nameValue,
            identifierValue,
            null,
            null!);

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsFailure);
        Assert.NotNull(createDepartmentResult.Error);
        Assert.Contains(createDepartmentResult.Error, e => e.Type == ErrorType.FAILURE);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(0, departmentsCount);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_empty_locations_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        string nameValue = "test_name";
        string identifierValue = "test";

        var request = new CreateDepartmentRequest(
            nameValue,
            identifierValue,
            null,
            []);

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsFailure);
        Assert.NotNull(createDepartmentResult.Error);
        Assert.Contains(createDepartmentResult.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(0, departmentsCount);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_non_existent_locations_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        string nameValue = "test_name";
        string identifierValue = "test";

        var request = new CreateDepartmentRequest(
            nameValue,
            identifierValue,
            null,
            [Guid.NewGuid()]);

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsFailure);
        Assert.NotNull(createDepartmentResult.Error);
        Assert.Contains(createDepartmentResult.Error, e => e.Type == ErrorType.NOT_FOUND);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(0, departmentsCount);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_inactive_locations_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync(isActive: false);

        string nameValue = "test_name";
        string identifierValue = "test";

        var request = new CreateDepartmentRequest(
            nameValue,
            identifierValue,
            null,
            [location.Id.Value]);

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsFailure);
        Assert.NotNull(createDepartmentResult.Error);
        Assert.Contains(createDepartmentResult.Error, e => e.Type == ErrorType.NOT_FOUND);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(0, departmentsCount);
        });
    }

    [Fact]
    public async Task CreateDepartment_child_with_inactive_parent_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        string parentNameValue = "parent_name";
        string parentIdentifierValue = "parent";

        var parentDepartment = await CreateDepartmentAsync([location.Id.Value], parentNameValue, parentIdentifierValue, isActive: false);

        string childNameValue = "child_name";
        string childIdentifierValue = "child";

        var request = new CreateDepartmentRequest(
            childNameValue,
            childIdentifierValue,
            parentDepartment.Id.Value,
            [location.Id.Value]);

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsFailure);
        Assert.NotNull(createDepartmentResult.Error);
        Assert.Contains(createDepartmentResult.Error, e => e.Type == ErrorType.NOT_FOUND);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(1, departmentsCount);
        });
    }

    [Fact]
    public async Task CreateDepartment_child_with_non_existent_parent_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        string parentNameValue = "parent_name";
        string parentIdentifierValue = "parent";

        var parentDepartment = await CreateDepartmentAsync([location.Id.Value], parentNameValue, parentIdentifierValue, isActive: false);

        string childNameValue = "child_name";
        string childIdentifierValue = "child";

        var request = new CreateDepartmentRequest(
            childNameValue,
            childIdentifierValue,
            parentDepartment.Id.Value,
            [location.Id.Value]);

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsFailure);
        Assert.NotNull(createDepartmentResult.Error);
        Assert.Contains(createDepartmentResult.Error, e => e.Type == ErrorType.NOT_FOUND);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(1, departmentsCount);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_empty_name_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        string nameValue = string.Empty;
        string identifierValue = "test";

        var request = new CreateDepartmentRequest(
            nameValue,
            identifierValue,
            null,
            [location.Id.Value]);

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsFailure);
        Assert.NotNull(createDepartmentResult.Error);
        Assert.Contains(createDepartmentResult.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(0, departmentsCount);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_invalid_name_length_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        string nameValue = "sh";
        string identifierValue = "test";

        var request = new CreateDepartmentRequest(
            nameValue,
            identifierValue,
            null,
            [location.Id.Value]);

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsFailure);
        Assert.NotNull(createDepartmentResult.Error);
        Assert.Contains(createDepartmentResult.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(0, departmentsCount);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_empty_identifier_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        string nameValue = "test_name";
        string identifierValue = string.Empty;

        var request = new CreateDepartmentRequest(
            nameValue,
            identifierValue,
            null,
            [location.Id.Value]);

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsFailure);
        Assert.NotNull(createDepartmentResult.Error);
        Assert.Contains(createDepartmentResult.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(0, departmentsCount);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_invalid_identifier_length_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        string nameValue = "test_name";
        string identifierValue = "sh";

        var request = new CreateDepartmentRequest(
            nameValue,
            identifierValue,
            null,
            [location.Id.Value]);

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsFailure);
        Assert.NotNull(createDepartmentResult.Error);
        Assert.Contains(createDepartmentResult.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(0, departmentsCount);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_invalid_identifier_character_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        string nameValue = "test_name";
        string identifierValue = "тест-на-русском";

        var request = new CreateDepartmentRequest(
            nameValue,
            identifierValue,
            null,
            [location.Id.Value]);

        // act
        var createDepartmentResponse = await AppHttpClient.PostAsJsonAsync("/api/Departments", request, cancellationToken);
        var createDepartmentResult = await createDepartmentResponse.HandleResponseAsync<Guid?>(cancellationToken);

        // assert
        Assert.True(createDepartmentResult.IsFailure);
        Assert.NotNull(createDepartmentResult.Error);
        Assert.Contains(createDepartmentResult.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(0, departmentsCount);
        });
    }
}