using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure.Postgres.Positions.Cleanup;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Positions.Services;

public class PositionsCleanupServiceTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task PositionsCleanupService_delete_inactive_position_older_than_threshold_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "dept", identifier: "dept");
        var position = await CreatePositionAsync([department.Id.Value]);
        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisablePositionAtDateAsync(position.Id, disableDate);

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(1, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            var deletedPosition = await dbContext.Positions
                .FirstOrDefaultAsync(p => p.Id == position.Id, cancellationToken);
            Assert.Null(deletedPosition);
        });
    }

    [Fact]
    public async Task PositionsCleanupService_not_delete_inactive_position_newer_than_threshold_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "dept", identifier: "dept");
        var position = await CreatePositionAsync([department.Id.Value]);
        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(25);
        await DisablePositionAtDateAsync(position.Id, disableDate);

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(0, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            var existingPosition = await dbContext.Positions
                .FirstOrDefaultAsync(p => p.Id == position.Id, cancellationToken);
            Assert.NotNull(existingPosition);
            Assert.False(existingPosition.IsActive);
            Assert.NotNull(existingPosition.DeletedAt);
        });
    }

    [Fact]
    public async Task PositionsCleanupService_delete_inactive_position_exactly_threshold_days_old_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "dept", identifier: "dept");
        var position = await CreatePositionAsync([department.Id.Value]);
        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(30);
        await DisablePositionAtDateAsync(position.Id, disableDate);

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(1, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            var deletedPosition = await dbContext.Positions
                .FirstOrDefaultAsync(p => p.Id == position.Id, cancellationToken);
            Assert.Null(deletedPosition);
        });
    }

    [Fact]
    public async Task PositionsCleanupService_delete_position_with_multiple_department_links_should_delete_all_links()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var department1 = await CreateDepartmentAsync([location.Id.Value], name: "dept_one", identifier: "deptone");
        var department2 = await CreateDepartmentAsync([location.Id.Value], name: "dept_two", identifier: "depttwo");
        var position = await CreatePositionAsync([department1.Id.Value, department2.Id.Value]);

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisablePositionAtDateAsync(position.Id, disableDate);

        // act
        await ExecuteCleanupAsync(cancellationToken);

        // assert
        await ExecuteInDb(async dbContext =>
        {
            int departmentPositionsCount = await dbContext.DepartmentPositions
                .CountAsync(dp => dp.PositionId == position.Id, cancellationToken);
            Assert.Equal(0, departmentPositionsCount);
        });
    }

    [Fact]
    public async Task PositionsCleanupService_not_delete_active_position_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "dept", identifier: "dept");
        var position = await CreatePositionAsync([department.Id.Value]);

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(0, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            var existingPosition = await dbContext.Positions
                .FirstOrDefaultAsync(p => p.Id == position.Id, cancellationToken);
            Assert.NotNull(existingPosition);
            Assert.True(existingPosition.IsActive);
        });
    }

    [Fact]
    public async Task PositionsCleanupService_not_delete_position_without_deleted_at_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "dept", identifier: "dept");
        var position = await CreatePositionAsync([department.Id.Value]);

        // Reset DeletedAt to null since Deactivate() sets it
        await ExecuteInDb(async dbContext =>
        {
            await dbContext.Positions
                .Where(p => p.Id == position.Id)
                .ExecuteUpdateAsync(
                    setter
                    => setter
                        .SetProperty(p => p.IsActive, false)
                        .SetProperty(p => p.DeletedAt, (DateTime?)null), cancellationToken: cancellationToken);
        });

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(0, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            var existingPosition = await dbContext.Positions
                .FirstOrDefaultAsync(p => p.Id == position.Id, cancellationToken);
            Assert.NotNull(existingPosition);
            Assert.False(existingPosition.IsActive);
            Assert.Null(existingPosition.DeletedAt);
        });
    }

    [Fact]
    public async Task PositionsCleanupService_not_delete_anything_when_no_positions_to_delete_should_return_zero()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(0, deletedCount);
    }

    [Fact]
    public async Task PositionsCleanupService_be_idempotent_when_called_multiple_times_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "dept", identifier: "dept");
        var position = await CreatePositionAsync([department.Id.Value]);
        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisablePositionAtDateAsync(position.Id, disableDate);

        // act
        int firstRunCount = await ExecuteCleanupAsync(cancellationToken);
        int secondRunCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(1, firstRunCount);
        Assert.Equal(0, secondRunCount);
    }

    [Fact]
    public async Task PositionsCleanupService_delete_position_not_affect_linked_departments()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "dept", identifier: "dept");
        var position = await CreatePositionAsync([department.Id.Value]);

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisablePositionAtDateAsync(position.Id, disableDate);

        // act
        await ExecuteCleanupAsync(cancellationToken);

        // assert
        await ExecuteInDb(async dbContext =>
        {
            bool departmentExists = await dbContext.Departments
                .AnyAsync(d => d.Id == department.Id, cancellationToken);
            Assert.True(departmentExists);
        });
    }

    [Fact]
    public async Task PositionsCleanupService_delete_multiple_positions_in_one_batch_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "dept", identifier: "dept");

        var positions = new List<Position>();
        for (int i = 0; i < 10; i++)
        {
            var position = await CreatePositionAsync([department.Id.Value], name: $"pos_{i}");
            var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
            await DisablePositionAtDateAsync(position.Id, disableDate);
            positions.Add(position);
        }

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(10, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            foreach (var position in positions)
            {
                bool exists = await dbContext.Positions
                    .AnyAsync(p => p.Id == position.Id, cancellationToken);
                Assert.False(exists);
            }
        });
    }

    [Fact]
    public async Task PositionsCleanupService_delete_only_positions_older_than_threshold_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "dept", identifier: "dept");

        var oldPosition = await CreatePositionAsync([department.Id.Value], name: "old_pos");
        var recentPosition = await CreatePositionAsync([department.Id.Value], name: "recent_pos");

        await DisablePositionAtDateAsync(oldPosition.Id, DateTime.UtcNow - TimeSpan.FromDays(31));
        await DisablePositionAtDateAsync(recentPosition.Id, DateTime.UtcNow - TimeSpan.FromDays(25));

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(1, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            bool oldExists = await dbContext.Positions
                .AnyAsync(p => p.Id == oldPosition.Id, cancellationToken);
            Assert.False(oldExists);

            bool recentExists = await dbContext.Positions
                .AnyAsync(p => p.Id == recentPosition.Id, cancellationToken);
            Assert.True(recentExists);
        });
    }

    private async Task DisablePositionAtDateAsync(PositionId positionId, DateTime deletedDate)
    {
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions.FirstAsync(p => p.Id == positionId);
            position.Deactivate();

            await dbContext.SaveChangesAsync();

            await dbContext.Positions
                .Where(p => p.Id == positionId)
                .ExecuteUpdateAsync(setter
                    => setter
                        .SetProperty(p => p.IsActive, false)
                        .SetProperty(p => p.DeletedAt, deletedDate));
        });
    }

    private async Task<int> ExecuteCleanupAsync(CancellationToken cancellationToken)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<PositionsCleanupService>();
        return await sut.CleanupAsync(cancellationToken);
    }
}