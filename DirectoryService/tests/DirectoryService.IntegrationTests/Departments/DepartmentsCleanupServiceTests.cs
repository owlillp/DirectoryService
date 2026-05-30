using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Postgres.Departments.Cleanup;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class DepartmentsCleanupServiceTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task DepartmentsCleanupService_delete_inactive_department_older_than_threshold_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        await CreatePositionAsync([department.Id.Value]);

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableDepartmentAtDateAsync(department.Id, disableDate);

        // act
        int deletedCount = await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        Assert.Equal(1, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            var deletedDepartment = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department.Id, cancellationToken);
            Assert.Null(deletedDepartment);
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_not_delete_inactive_department_newer_than_threshold_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(25);
        await DisableDepartmentAtDateAsync(department.Id, disableDate);

        // act
        int deletedCount = await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        Assert.Equal(0, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            var existingDepartment = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department.Id, cancellationToken);
            Assert.NotNull(existingDepartment);
            Assert.False(existingDepartment.IsActive);
            Assert.NotNull(existingDepartment.DeletedAt);
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_delete_inactive_department_exactly_threshold_days_old_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(30);
        await DisableDepartmentAtDateAsync(department.Id, disableDate);

        // act
        int deletedCount = await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        Assert.Equal(1, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            var deletedDepartment = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department.Id, cancellationToken);
            Assert.Null(deletedDepartment);
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_delete_department_with_several_locations_should_delete_all_links()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var locations = new List<Location>();
        for (int i = 0; i < 5; i++)
        {
            var location = await CreateLocationAsync(name: $"loc_{i}", city: $"city_{i}");
            locations.Add(location);
        }

        var department = await CreateDepartmentAsync(locations.Select(l => l.Id.Value));
        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableDepartmentAtDateAsync(department.Id, disableDate);

        // act
        await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        await ExecuteInDb(async dbContext =>
        {
            int departmentLocationsCount = await dbContext.DepartmentLocations
                .CountAsync(dl => dl.DepartmentId == department.Id, cancellationToken);
            Assert.Equal(0, departmentLocationsCount);
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_delete_department_with_several_positions_should_delete_all_links()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        for (int i = 0; i < 5; i++)
        {
            await CreatePositionAsync([department.Id.Value], name: $"pos_{i}");
        }

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableDepartmentAtDateAsync(department.Id, disableDate);

        // act
        await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        await ExecuteInDb(async dbContext =>
        {
            int departmentPositionsCount = await dbContext.DepartmentPositions
                .CountAsync(dp => dp.DepartmentId == department.Id, cancellationToken);
            Assert.Equal(0, departmentPositionsCount);
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_delete_department_without_children_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        await CreatePositionAsync([department.Id.Value]);

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableDepartmentAtDateAsync(department.Id, disableDate);

        // act
        await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var deletedDepartment = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department.Id, cancellationToken);
            Assert.Null(deletedDepartment);
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_delete_department_without_children_with_several_references_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var locations = new List<Location>();
        for (int i = 0; i < 5; i++)
        {
            var location = await CreateLocationAsync(name: $"test_{i}", city: $"city_{i}");
            locations.Add(location);
        }

        var department = await CreateDepartmentAsync(locations.Select(l => l.Id.Value));

        for (int i = 0; i < 5; i++)
        {
            await CreatePositionAsync([department.Id.Value], name: $"test_{i}");
        }

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableDepartmentAtDateAsync(department.Id, disableDate);

        // act
        await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var deletedDepartment = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department.Id, cancellationToken);

            int locationsCount = await dbContext.DepartmentLocations
                .CountAsync(l => l.DepartmentId == department.Id, cancellationToken);

            int positionsCount = await dbContext.DepartmentPositions
                .CountAsync(p => p.DepartmentId == department.Id, cancellationToken);

            Assert.Null(deletedDepartment);
            Assert.Equal(0, locationsCount);
            Assert.Equal(0, positionsCount);
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_not_delete_child_departments_when_parent_deleted_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

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

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableDepartmentAtDateAsync(parentDepartment.Id, disableDate);

        // act
        await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        await ExecuteInDb(async dbContext =>
        {
            bool parentExists = await dbContext.Departments
                .AnyAsync(d => d.Id == parentDepartment.Id, cancellationToken);
            Assert.False(parentExists);

            bool childExists = await dbContext.Departments
                .AnyAsync(d => d.Id == childDepartment.Id, cancellationToken);
            Assert.True(childExists);

            bool grandChildExists = await dbContext.Departments
                .AnyAsync(d => d.Id == grandChildDepartment.Id, cancellationToken);
            Assert.True(grandChildExists);
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_update_path_for_child_departments_when_parent_deleted_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var location = await CreateLocationAsync();
        var parentDepartment = await CreateDepartmentAsync([location.Id.Value], name: "parent", identifier: "parent");
        var childDepartment = await CreateDepartmentAsync(
            [location.Id.Value],
            name: "child",
            identifier: "child",
            parent: parentDepartment);

        string originalChildPath = childDepartment.Path.Value;
        Assert.Contains(parentDepartment.Path.Value, originalChildPath);

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableDepartmentAtDateAsync(parentDepartment.Id, disableDate);

        // act
        await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var updatedChild = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == childDepartment.Id, cancellationToken);

            Assert.NotNull(updatedChild);
            Assert.DoesNotContain(parentDepartment.Path.Value, updatedChild.Path.Value);
            Assert.DoesNotContain("deleted_", updatedChild.Path.Value);
            Assert.NotEqual(originalChildPath, updatedChild.Path.Value);
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_update_parent_id_for_direct_children_when_parent_deleted_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var location = await CreateLocationAsync();
        var grandparentDepartment = await CreateDepartmentAsync([location.Id.Value], name: "grandparent", identifier: "grandparent");
        var parentDepartment = await CreateDepartmentAsync(
            [location.Id.Value],
            name: "parent",
            identifier: "parent",
            parent: grandparentDepartment);
        var childDepartment = await CreateDepartmentAsync(
            [location.Id.Value],
            name: "child",
            identifier: "child",
            parent: parentDepartment);

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableDepartmentAtDateAsync(parentDepartment.Id, disableDate);

        // act
        await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var updatedChild = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == childDepartment.Id, cancellationToken);

            Assert.NotNull(updatedChild);
            Assert.Equal(grandparentDepartment.Id, updatedChild.ParentId);
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_update_path_for_deep_hierarchy_when_intermediate_parent_deleted_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var location = await CreateLocationAsync();

        var departments = new List<Department>();
        Department? previousDept = null;

        string[] identifiers =
        [
            "one",
            "two",
            "three",
            "four",
            "five"
        ];
        for (int i = 0; i < 5; i++)
        {
            var dept = await CreateDepartmentAsync(
                [location.Id.Value],
                name: $"level_{i}",
                identifier: identifiers[i],
                parent: previousDept);
            departments.Add(dept);
            previousDept = dept;
        }

        var deptToDelete = departments[2];
        var originalPaths = departments.Skip(3).Select(d => d.Path.Value).ToList();

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableDepartmentAtDateAsync(deptToDelete.Id, disableDate);

        // act
        await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        await ExecuteInDb(async dbContext =>
        {
            bool deletedExists = await dbContext.Departments
                .AnyAsync(d => d.Id == deptToDelete.Id, cancellationToken);
            Assert.False(deletedExists);

            for (int i = 3; i < departments.Count; i++)
            {
                var child = await dbContext.Departments
                    .FirstOrDefaultAsync(d => d.Id == departments[i].Id, cancellationToken);
                Assert.NotNull(child);

                if (i == 3)
                {
                    Assert.Equal(departments[1].Id, child.ParentId);
                }

                Assert.DoesNotContain(deptToDelete.Path.Value, child.Path.Value);
                Assert.DoesNotContain("deleted_", child.Path.Value);

                Assert.True(child.Path.Value.Length < originalPaths[i - 3].Length);
            }
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_delete_multiple_parents_in_same_tree_should_handle_correctly()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var location = await CreateLocationAsync();

        var deptA = await CreateDepartmentAsync([location.Id.Value], name: "aaaaa", identifier: "aaa");
        var deptB = await CreateDepartmentAsync([location.Id.Value], name: "bbbbb", identifier: "bbb", parent: deptA);
        var deptC = await CreateDepartmentAsync([location.Id.Value], name: "ccccc", identifier: "ccc", parent: deptB);
        var deptD = await CreateDepartmentAsync([location.Id.Value], name: "ddddd", identifier: "ddd", parent: deptC);

        var oldDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableDepartmentAtDateAsync(deptA.Id, oldDate);
        await DisableDepartmentAtDateAsync(deptB.Id, oldDate);

        var disableDateC = DateTime.UtcNow - TimeSpan.FromDays(5);
        await DisableDepartmentAtDateAsync(deptC.Id, disableDateC);

        // act
        int deletedCount = await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        Assert.Equal(2, deletedCount);

        await ExecuteInDb(async dbContext =>
        {
            bool aExists = await dbContext.Departments.AnyAsync(d => d.Id == deptA.Id, cancellationToken);
            bool bExists = await dbContext.Departments.AnyAsync(d => d.Id == deptB.Id, cancellationToken);
            bool cExists = await dbContext.Departments.AnyAsync(d => d.Id == deptC.Id, cancellationToken);
            bool dExists = await dbContext.Departments.AnyAsync(d => d.Id == deptD.Id, cancellationToken);

            Assert.False(aExists);
            Assert.False(bExists);
            Assert.True(cExists);
            Assert.True(dExists);

            if (cExists && dExists)
            {
                var deptCUpdated = await dbContext.Departments.FirstAsync(d => d.Id == deptC.Id, cancellationToken);
                var deptDUpdated = await dbContext.Departments.FirstAsync(d => d.Id == deptD.Id, cancellationToken);

                Assert.Null(deptCUpdated.ParentId);
                Assert.Equal(deptC.Id, deptDUpdated.ParentId);
                Assert.Equal(0, deptCUpdated.Depth);
                Assert.Equal(1, deptDUpdated.Depth);
                Assert.Equal("deleted_ccc", deptCUpdated.Path.Value);
                Assert.Equal("deleted_ccc.ddd", deptDUpdated.Path.Value);
            }
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_delete_multiple_independent_departments_in_one_batch_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var location = await CreateLocationAsync();
        var departments = new List<Department>();

        string[] identifiers =
        [
            "one",
            "two",
            "three",
            "four",
            "five",
            "six",
            "seven",
            "eight",
            "nine",
            "ten"
        ];
        for (int i = 0; i < 10; i++)
        {
            var dept = await CreateDepartmentAsync([location.Id.Value], name: $"dept_{i}", identifier: identifiers[i]);
            await CreatePositionAsync([dept.Id.Value], name: $"pos_{i}");
            var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
            await DisableDepartmentAtDateAsync(dept.Id, disableDate);
            departments.Add(dept);
        }

        // act
        int deletedCount = await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        Assert.Equal(10, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            foreach (var dept in departments)
            {
                bool exists = await dbContext.Departments.AnyAsync(d => d.Id == dept.Id, cancellationToken);
                Assert.False(exists);
            }
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_not_delete_anything_when_no_departments_to_delete_should_return_zero()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        // act
        int deletedCount = await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        Assert.Equal(0, deletedCount);
    }

    [Fact]
    public async Task DepartmentsCleanupService_be_idempotent_when_called_multiple_times_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableDepartmentAtDateAsync(department.Id, disableDate);

        // act
        int firstRunCount = await ExecuteCleanupAsync(thresholdDays, cancellationToken);
        int secondRunCount = await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        Assert.Equal(1, firstRunCount);
        Assert.Equal(0, secondRunCount);
    }

    [Fact]
    public async Task DepartmentsCleanupService_update_child_timestamps_when_reparented_should_success()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var location = await CreateLocationAsync();
        var parentDepartment = await CreateDepartmentAsync([location.Id.Value], name: "parent", identifier: "parent");
        var childDepartment = await CreateDepartmentAsync(
            [location.Id.Value],
            name: "child",
            identifier: "child",
            parent: parentDepartment);

        var originalUpdatedAt = childDepartment.UpdatedAt;

        await Task.Delay(100, cancellationToken);

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableDepartmentAtDateAsync(parentDepartment.Id, disableDate);

        // act
        await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var updatedChild = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == childDepartment.Id, cancellationToken);

            Assert.NotNull(updatedChild);
            Assert.True(updatedChild.UpdatedAt > originalUpdatedAt);
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_handle_department_with_child_that_has_same_path_prefix_should_correctly_update()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 30;

        var location = await CreateLocationAsync();

        var dept1 = await CreateDepartmentAsync([location.Id.Value], name: "test", identifier: "test");
        var child1 = await CreateDepartmentAsync([location.Id.Value], name: "child1", identifier: "childone", parent: dept1);

        var dept2 = await CreateDepartmentAsync([location.Id.Value], name: "test2", identifier: "testtwo");
        var child2 = await CreateDepartmentAsync([location.Id.Value], name: "child2", identifier: "childtwo", parent: dept2);

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(31);
        await DisableDepartmentAtDateAsync(dept1.Id, disableDate);

        // act
        int deleteCount = await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        Assert.Equal(1, deleteCount);

        await ExecuteInDb(async dbContext =>
        {
            bool deletedDept1 = await dbContext.Departments
                .AnyAsync(d => d.Id == dept1.Id, cancellationToken);
            Assert.False(deletedDept1);

            var updatedChild1 = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == child1.Id, cancellationToken);
            Assert.NotNull(updatedChild1);
            Assert.Null(updatedChild1.ParentId);

            var existingDept2 = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == dept2.Id, cancellationToken);
            Assert.NotNull(existingDept2);

            var existingChild2 = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == child2.Id, cancellationToken);
            Assert.NotNull(existingChild2);
            Assert.Equal(dept2.Id, existingChild2.ParentId);
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_delete_department_with_threshold_zero_should_delete_all_inactive()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 0;

        var location = await CreateLocationAsync();
        var department1 = await CreateDepartmentAsync([location.Id.Value], name: "dept1");
        var department2 = await CreateDepartmentAsync([location.Id.Value], name: "dept2");

        var disableDate1 = DateTime.UtcNow - TimeSpan.FromDays(1);
        var disableDate2 = DateTime.UtcNow - TimeSpan.FromHours(1);

        await DisableDepartmentAtDateAsync(department1.Id, disableDate1);
        await DisableDepartmentAtDateAsync(department2.Id, disableDate2);

        // act
        int deletedCount = await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        Assert.Equal(2, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            bool dept1Exists = await dbContext.Departments.AnyAsync(d => d.Id == department1.Id, cancellationToken);
            bool dept2Exists = await dbContext.Departments.AnyAsync(d => d.Id == department2.Id, cancellationToken);

            Assert.False(dept1Exists);
            Assert.False(dept2Exists);
        });
    }

    [Fact]
    public async Task DepartmentsCleanupService_delete_department_with_large_threshold_should_not_delete_any()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        int thresholdDays = 365;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        var disableDate = DateTime.UtcNow - TimeSpan.FromDays(100);
        await DisableDepartmentAtDateAsync(department.Id, disableDate);

        // act
        int deletedCount = await ExecuteCleanupAsync(thresholdDays, cancellationToken);

        // assert
        Assert.Equal(0, deletedCount);
        await ExecuteInDb(async dbContext =>
        {
            var existingDepartment = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == department.Id, cancellationToken);
            Assert.NotNull(existingDepartment);
        });
    }

    private async Task<int> ExecuteCleanupAsync(int thresholdDays, CancellationToken cancellationToken)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<DepartmentsCleanupService>();
        return await sut.CleanupAsync(cancellationToken);
    }
}