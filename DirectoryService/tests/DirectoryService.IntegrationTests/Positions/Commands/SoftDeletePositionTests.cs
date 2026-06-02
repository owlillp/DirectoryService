using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace DirectoryService.IntegrationTests.Positions.Commands;

public class SoftDeletePositionTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task SoftDeletePosition_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([department.Id.Value]);

        // act
        var response = await AppHttpClient.DeleteAsync($"/api/Positions/{position.Id.Value}", cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var targetPosition = await dbContext.Positions
                .FirstOrDefaultAsync(p => p.Id == position.Id, cancellationToken);

            Assert.NotNull(targetPosition);
            Assert.False(targetPosition.IsActive);
            Assert.NotNull(targetPosition.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeletePosition_with_unique_references_should_deactivate_shared_resources()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([department.Id.Value]);

        // act
        var response = await AppHttpClient.DeleteAsync($"/api/Positions/{position.Id.Value}", cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var targetPosition = await dbContext.Positions
                .Include(p => p.Departments)
                .FirstOrDefaultAsync(p => p.Id == position.Id, cancellationToken);

            Assert.NotNull(targetPosition);
            Assert.False(targetPosition.IsActive);
            Assert.NotNull(targetPosition.DeletedAt);
            Assert.Single(targetPosition.Departments);
        });
    }

    [Fact]
    public async Task SoftDeletePosition_shared_with_other_departments_should_not_deactivate()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department1 = await CreateDepartmentAsync([location.Id.Value], name: "dept_one", identifier: "deptone");
        var department2 = await CreateDepartmentAsync([location.Id.Value], name: "dept_two", identifier: "depttwo");

        var position = await CreatePositionAsync([department1.Id.Value, department2.Id.Value]);

        // remove position from department1 only
        var deleteResponse = await AppHttpClient.DeleteAsync($"/api/Positions/{position.Id.Value}", cancellationToken);
        await deleteResponse.HandleResponseAsync(cancellationToken);

        // act
        await ExecuteInDb(async dbContext =>
        {
            var targetPosition = await dbContext.Positions
                .FirstOrDefaultAsync(p => p.Id == position.Id, cancellationToken);

            Assert.NotNull(targetPosition);
            Assert.False(targetPosition.IsActive);
            Assert.NotNull(targetPosition.DeletedAt);

            var targetDepartment = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department1.Id, cancellationToken);
            Assert.NotNull(targetDepartment);

            var department2Target = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department2.Id, cancellationToken);
            Assert.NotNull(department2Target);
            Assert.True(department2Target.IsActive);
        });
    }

    [Fact]
    public async Task SoftDeletePosition_when_position_not_found_should_return_not_found_error()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var nonExistentId = Guid.NewGuid();

        // act
        var response = await AppHttpClient.DeleteAsync($"/api/Positions/{nonExistentId}", cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task SoftDeletePosition_when_position_already_deleted_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([department.Id.Value]);

        var firstDeleteResponse = await AppHttpClient.DeleteAsync($"/api/Positions/{position.Id.Value}", cancellationToken);
        var firstDeleteResult = await firstDeleteResponse.HandleResponseAsync(cancellationToken);
        Assert.True(firstDeleteResult.IsSuccess);

        // act
        var secondDeleteResponse = await AppHttpClient.DeleteAsync($"/api/Positions/{position.Id.Value}", cancellationToken);
        var secondDeleteResult = await secondDeleteResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(secondDeleteResult.IsFailure);
        Assert.NotNull(secondDeleteResult.Error);
        Assert.Contains(secondDeleteResult.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task SoftDeletePosition_with_invalid_guid_should_return_validation_error()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var invalidId = Guid.Empty;

        // act
        var response = await AppHttpClient.DeleteAsync($"/api/Positions/{invalidId}", cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }
}