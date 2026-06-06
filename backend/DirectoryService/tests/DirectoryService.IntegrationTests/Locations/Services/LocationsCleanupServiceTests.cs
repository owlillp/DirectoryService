using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Postgres.Locations.Cleanup;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Locations.Services;

public class LocationsCleanupServiceTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task LocationsCleanupService_delete_inactive_location_older_than_threshold_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableLocationAtDateAsync(location.Id, disableDate);

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(1, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            var deletedLocation = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == location.Id, cancellationToken);
            Assert.Null(deletedLocation);
        });
    }

    [Fact]
    public async Task LocationsCleanupService_not_delete_inactive_location_newer_than_threshold_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(25);
        await DisableLocationAtDateAsync(location.Id, disableDate);

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(0, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            var existingLocation = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == location.Id, cancellationToken);
            Assert.NotNull(existingLocation);
            Assert.False(existingLocation.IsActive);
            Assert.NotNull(existingLocation.DeletedAt);
        });
    }

    [Fact]
    public async Task LocationsCleanupService_delete_inactive_location_exactly_threshold_days_old_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(30);
        await DisableLocationAtDateAsync(location.Id, disableDate);

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(1, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            var deletedLocation = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == location.Id, cancellationToken);
            Assert.Null(deletedLocation);
        });
    }

    [Fact]
    public async Task LocationsCleanupService_delete_location_with_multiple_department_links_should_delete_all_links()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        await CreateDepartmentAsync([location.Id.Value], name: "dept_one", identifier: "deptone");
        await CreateDepartmentAsync([location.Id.Value], name: "dept_two", identifier: "depttwo");

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableLocationAtDateAsync(location.Id, disableDate);

        // act
        await ExecuteCleanupAsync(cancellationToken);

        // assert
        await ExecuteInDb(async dbContext =>
        {
            int departmentLocationsCount = await dbContext.DepartmentLocations
                .CountAsync(dl => dl.LocationId == location.Id, cancellationToken);
            Assert.Equal(0, departmentLocationsCount);
        });
    }

    [Fact]
    public async Task LocationsCleanupService_not_delete_active_location_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync(isActive: true);

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(0, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            var existingLocation = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == location.Id, cancellationToken);
            Assert.NotNull(existingLocation);
            Assert.True(existingLocation.IsActive);
        });
    }

    [Fact]
    public async Task LocationsCleanupService_not_delete_location_without_deleted_at_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync(isActive: false);

        // Reset DeletedAt to null since Deactivate() sets it
        await ExecuteInDb(async dbContext =>
        {
            await dbContext.Locations
                .Where(l => l.Id == location.Id)
                .ExecuteUpdateAsync(
                    setter
                    => setter
                        .SetProperty(l => l.IsActive, false)
                        .SetProperty(l => l.DeletedAt, (DateTime?)null), cancellationToken: cancellationToken);
        });

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(0, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            var existingLocation = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == location.Id, cancellationToken);
            Assert.NotNull(existingLocation);
            Assert.False(existingLocation.IsActive);
            Assert.Null(existingLocation.DeletedAt);
        });
    }

    [Fact]
    public async Task LocationsCleanupService_not_delete_anything_when_no_locations_to_delete_should_return_zero()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(0, deletedCount);
    }

    [Fact]
    public async Task LocationsCleanupService_be_idempotent_when_called_multiple_times_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableLocationAtDateAsync(location.Id, disableDate);

        // act
        int firstRunCount = await ExecuteCleanupAsync(cancellationToken);
        int secondRunCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(1, firstRunCount);
        Assert.Equal(0, secondRunCount);
    }

    [Fact]
    public async Task LocationsCleanupService_delete_location_not_affect_linked_departments()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value], name: "dept", identifier: "dept");

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableLocationAtDateAsync(location.Id, disableDate);

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
    public async Task LocationsCleanupService_delete_multiple_locations_in_one_batch_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var locations = new List<Location>();
        for (int i = 0; i < 10; i++)
        {
            var location = await CreateLocationAsync(name: $"loc_{i}", city: $"city_{i}");
            var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
            await DisableLocationAtDateAsync(location.Id, disableDate);
            locations.Add(location);
        }

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(10, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            foreach (var location in locations)
            {
                bool exists = await dbContext.Locations
                    .AnyAsync(l => l.Id == location.Id, cancellationToken);
                Assert.False(exists);
            }
        });
    }

    [Fact]
    public async Task LocationsCleanupService_delete_only_locations_older_than_threshold_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var oldLocation = await CreateLocationAsync(name: "old_loc", city: "old_city");
        var recentLocation = await CreateLocationAsync(name: "recent_loc", city: "recent_city");

        await DisableLocationAtDateAsync(oldLocation.Id, DateTime.UtcNow - TimeSpan.FromDays(31));
        await DisableLocationAtDateAsync(recentLocation.Id, DateTime.UtcNow - TimeSpan.FromDays(25));

        // act
        int deletedCount = await ExecuteCleanupAsync(cancellationToken);

        // assert
        Assert.Equal(1, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            bool oldExists = await dbContext.Locations
                .AnyAsync(l => l.Id == oldLocation.Id, cancellationToken);
            Assert.False(oldExists);

            bool recentExists = await dbContext.Locations
                .AnyAsync(l => l.Id == recentLocation.Id, cancellationToken);
            Assert.True(recentExists);
        });
    }

    private async Task DisableLocationAtDateAsync(LocationId locationId, DateTime deletedDate)
    {
        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations.FirstAsync(l => l.Id == locationId);
            location.Deactivate();

            await dbContext.SaveChangesAsync();

            await dbContext.Locations
                .Where(l => l.Id == locationId)
                .ExecuteUpdateAsync(setter
                    => setter
                        .SetProperty(l => l.IsActive, false)
                        .SetProperty(l => l.DeletedAt, deletedDate));
        });
    }

    private async Task<int> ExecuteCleanupAsync(CancellationToken cancellationToken)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<LocationsCleanupService>();
        return await sut.CleanupAsync(cancellationToken);
    }
}