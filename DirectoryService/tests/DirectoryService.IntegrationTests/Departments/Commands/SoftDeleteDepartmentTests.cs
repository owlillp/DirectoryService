using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Departments.Commands;

public class SoftDeleteDepartmentTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task SoftDeleteDepartment_with_unique_references_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        await CreatePositionAsync([department.Id.Value]);

        // act
        var response = await AppHttpClient.DeleteAsync(BuildUrl(department.Id.Value), cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var targetDepartment = await dbContext.Departments
                .Include(d => d.Locations)
                .Include(d => d.Positions)
                .FirstOrDefaultAsync(
                    d => d.Id == department.Id,
                    cancellationToken: cancellationToken);

            Assert.NotNull(targetDepartment);
            Assert.StartsWith("deleted_", targetDepartment.Path.Value);

            var locationIds = targetDepartment.Locations.Select(dl => dl.LocationId);
            var positionIds = targetDepartment.Positions.Select(dp => dp.PositionId);

            var targetPositions = await dbContext.Positions
                .Where(p => positionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            var targetLocations = await dbContext.Locations
                .Where(l => locationIds.Contains(l.Id))
                .ToListAsync(cancellationToken);

            Assert.False(targetDepartment.IsActive);
            Assert.NotNull(targetDepartment.DeletedAt);
            Assert.Single(targetDepartment.Locations);
            Assert.Single(targetDepartment.Positions);
            Assert.Equal(targetPositions.Count, targetDepartment.Positions.Count);
            Assert.Equal(targetLocations.Count, targetDepartment.Locations.Count);
            Assert.True(targetLocations.All(l => !l.IsActive));
            Assert.True(targetPositions.All(p => !p.IsActive));
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_shared_references_should_not_deactivate_shared_locations_and_positions()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();

        var department1 = await CreateDepartmentAsync([location.Id.Value], name: "department1", identifier: "dept_one");
        var department2 = await CreateDepartmentAsync([location.Id.Value], name: "department2", identifier: "dept_two");

        var position = await CreatePositionAsync([department1.Id.Value, department2.Id.Value]);

        // act
        var response = await AppHttpClient.DeleteAsync(BuildUrl(department1.Id.Value), cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var deletedDepartment = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department1.Id, cancellationToken);

            Assert.NotNull(deletedDepartment);
            Assert.False(deletedDepartment.IsActive);
            Assert.NotNull(deletedDepartment.DeletedAt);

            var activeDepartment = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department2.Id, cancellationToken);

            Assert.NotNull(activeDepartment);
            Assert.True(activeDepartment.IsActive);
            Assert.Null(activeDepartment.DeletedAt);

            var targetLocation = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == location.Id, cancellationToken);

            Assert.NotNull(targetLocation);
            Assert.True(targetLocation.IsActive);
            Assert.Null(targetLocation.DeletedAt);

            var targetPosition = await dbContext.Positions
                .FirstOrDefaultAsync(p => p.Id == position.Id, cancellationToken);

            Assert.NotNull(targetPosition);
            Assert.True(targetPosition.IsActive);
            Assert.Null(targetPosition.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_child_departments_should_update_their_paths_but_not_delete_them()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var parentDepartment = await CreateDepartmentAsync([location.Id.Value], name: "parent", identifier: "parent");
        var childDepartment = await CreateDepartmentAsync(
            [location.Id.Value],
            name: "child",
            identifier: "child",
            parent: parentDepartment);
        var grandChildDepartment = await CreateDepartmentAsync(
            [location.Id.Value],
            name: "grandchild",
            identifier: "grandchild",
            parent: childDepartment);

        // act
        var response = await AppHttpClient.DeleteAsync(BuildUrl(parentDepartment.Id.Value), cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var deletedParent = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == parentDepartment.Id, cancellationToken);

            Assert.NotNull(deletedParent);
            Assert.False(deletedParent.IsActive);
            Assert.StartsWith("deleted_", deletedParent.Path.Value);
            Assert.NotNull(deletedParent.DeletedAt);

            var child = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == childDepartment.Id, cancellationToken);

            Assert.NotNull(child);
            Assert.True(child.IsActive, "Child department should remain active");
            Assert.Null(child.DeletedAt);

            Assert.StartsWith(deletedParent.Path.Value, child.Path.Value);

            var grandChild = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == grandChildDepartment.Id, cancellationToken);

            Assert.NotNull(grandChild);
            Assert.True(grandChild.IsActive, "Grandchild department should remain active");
            Assert.Null(grandChild.DeletedAt);
            Assert.StartsWith(deletedParent.Path.Value, grandChild.Path.Value);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_when_department_not_found_should_return_not_found_error()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var nonExistentId = Guid.NewGuid();

        // act
        var response = await AppHttpClient.DeleteAsync(BuildUrl(nonExistentId), cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task SoftDeleteDepartment_when_department_already_deleted_should_succeed_without_changes()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        var firstDeleteResponse = await AppHttpClient.DeleteAsync(BuildUrl(department.Id.Value), cancellationToken);
        var firstDeleteResult = await firstDeleteResponse.HandleResponseAsync(cancellationToken);
        Assert.True(firstDeleteResult.IsSuccess);

        Department? departmentAfterFirstDelete;
        DateTime? firstDeletedAt = null;
        string? firstPath = null;

        await ExecuteInDb(async dbContext =>
        {
            departmentAfterFirstDelete = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department.Id, cancellationToken);
            firstDeletedAt = departmentAfterFirstDelete!.DeletedAt;
            firstPath = departmentAfterFirstDelete.Path.Value;
            return Task.CompletedTask;
        });

        // act
        var secondDeleteResponse = await AppHttpClient.DeleteAsync(BuildUrl(department.Id.Value), cancellationToken);
        var secondDeleteResult = await secondDeleteResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(secondDeleteResult.IsFailure);
        Assert.NotNull(secondDeleteResult.Error);
        Assert.Contains(secondDeleteResult.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            var departmentAfterSecondDelete = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department.Id, cancellationToken);

            Assert.NotNull(departmentAfterSecondDelete);
            Assert.False(departmentAfterSecondDelete.IsActive);
            Assert.Equal(firstDeletedAt, departmentAfterSecondDelete.DeletedAt);
            Assert.Equal(firstPath, departmentAfterSecondDelete.Path.Value);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_invalid_guid_should_return_validation_error()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var invalidId = Guid.Empty;

        // act
        var response = await AppHttpClient.DeleteAsync(BuildUrl(invalidId), cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_multiple_locations_and_positions_should_deactivate_all_unused()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location1 = await CreateLocationAsync(name: "location1", city: "city1");
        var location2 = await CreateLocationAsync(name: "location2", city: "city2");
        var location3 = await CreateLocationAsync(name: "location3", city: "city3");

        var department = await CreateDepartmentAsync([location1.Id.Value, location2.Id.Value, location3.Id.Value]);

        await CreatePositionAsync([department.Id.Value], name: "position1");
        await CreatePositionAsync([department.Id.Value], name: "position2");
        await CreatePositionAsync([department.Id.Value], name: "position3");

        // act
        var response = await AppHttpClient.DeleteAsync(BuildUrl(department.Id.Value), cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var targetDepartment = await dbContext.Departments
                .Include(d => d.Locations)
                .Include(d => d.Positions)
                .FirstOrDefaultAsync(d => d.Id == department.Id, cancellationToken);

            Assert.NotNull(targetDepartment);
            Assert.False(targetDepartment.IsActive);
            Assert.Equal(3, targetDepartment.Locations.Count);
            Assert.Equal(3, targetDepartment.Positions.Count);

            var locationIds = targetDepartment.Locations.Select(dl => dl.LocationId);
            var deactivatedLocations = await dbContext.Locations
                .Where(l => locationIds.Contains(l.Id))
                .ToListAsync(cancellationToken);

            Assert.All(deactivatedLocations, l => Assert.False(l.IsActive));
            Assert.All(deactivatedLocations, l => Assert.NotNull(l.DeletedAt));

            var positionIds = targetDepartment.Positions.Select(dp => dp.PositionId);

            var deactivatedPositions = await dbContext.Positions
                .Where(p => positionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            Assert.All(deactivatedPositions, p => Assert.False(p.IsActive));
            Assert.All(deactivatedPositions, p => Assert.NotNull(p.DeletedAt));
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_concurrent_requests_should_handle_locking_correctly()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        // act
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(AppHttpClient.DeleteAsync(BuildUrl(department.Id.Value), cancellationToken));
        }

        var responses = await Task.WhenAll(tasks);
        var results = new List<UnitResult<Errors>>();

        foreach (var response in responses)
        {
            var result = await response.HandleResponseAsync(cancellationToken);
            results.Add(result);
        }

        // assert
        int successCount = results.Count(r => r.IsSuccess);
        int failureCount = results.Count(r => r.IsFailure);

        Assert.Equal(1, successCount);
        Assert.Equal(4, failureCount);

        await ExecuteInDb(async dbContext =>
        {
            var targetDepartment = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department.Id, cancellationToken: cancellationToken);

            Assert.NotNull(targetDepartment);
            Assert.False(targetDepartment.IsActive);
            Assert.NotNull(targetDepartment.DeletedAt);
        });
    }

    private static string BuildUrl(Guid departmentId)
        => $"/api/Departments/{departmentId}";
}