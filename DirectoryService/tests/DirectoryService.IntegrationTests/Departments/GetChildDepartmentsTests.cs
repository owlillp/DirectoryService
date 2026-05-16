using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Departments;

public class GetChildDepartmentsTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task GetChildDepartments_with_empty_database_should_succeed()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var parentId = Guid.NewGuid();

        var response = await AppHttpClient.GetAsync(BuildChildUrl(parentId), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Departments);
        Assert.Equal(0L, result.Value.Count);
    }

    [Fact]
    public async Task GetChildDepartments_should_return_children_of_specified_parent()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var parent = await CreateDepartmentAsync([location.Id.Value], "parent_dept", "parentdept");
        var childA = await CreateDepartmentAsync([location.Id.Value], "child_a", "childa", parent: parent);
        var childB = await CreateDepartmentAsync([location.Id.Value], "child_b", "childb", parent: parent);

        var response = await AppHttpClient.GetAsync(BuildChildUrl(parent.Id.Value, page: 1, size: 10), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2L, result.Value.Count);

        var childIds = result.Value.Departments.Select(c => c.Id).ToHashSet();
        Assert.Contains(childA.Id.Value, childIds);
        Assert.Contains(childB.Id.Value, childIds);
        Assert.All(result.Value.Departments, c => Assert.Equal(parent.Id.Value, c.ParentId));
        Assert.All(result.Value.Departments, c => Assert.Empty(c.Children));
    }

    [Fact]
    public async Task GetChildDepartments_should_not_return_non_child_departments()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var parent = await CreateDepartmentAsync([location.Id.Value], "parent_dept", "parentdept");
        var child = await CreateDepartmentAsync([location.Id.Value], "child_dept", "childdept", parent: parent);
        var unrelatedRoot = await CreateDepartmentAsync([location.Id.Value], "unrelated_root", "unrelatedroot");
        await CreateDepartmentAsync([location.Id.Value], "unrelated_child", "unrelatedchild", parent: unrelatedRoot);

        var response = await AppHttpClient.GetAsync(BuildChildUrl(parent.Id.Value), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1L, result.Value.Count);
        Assert.Equal(child.Id.Value, result.Value.Departments[0].Id);
    }

    [Fact]
    public async Task GetChildDepartments_should_return_empty_for_parent_without_children()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var parent = await CreateDepartmentAsync([location.Id.Value], "parent_dept", "parentdept");

        var response = await AppHttpClient.GetAsync(BuildChildUrl(parent.Id.Value), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Departments);
        Assert.Equal(0L, result.Value.Count);
    }

    [Fact]
    public async Task GetChildDepartments_should_return_total_child_count_across_pages()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var parent = await CreateDepartmentAsync([location.Id.Value], "parent_dept", "parentdept");
        var childA = await CreateDepartmentAsync([location.Id.Value], "child_a", "childa", parent: parent);
        var childB = await CreateDepartmentAsync([location.Id.Value], "child_b", "childb", parent: parent);
        var childC = await CreateDepartmentAsync([location.Id.Value], "child_c", "childc", parent: parent);

        var pageOneResponse = await AppHttpClient.GetAsync(BuildChildUrl(parent.Id.Value, page: 1, size: 1), cancellationToken);
        var pageOne = await pageOneResponse.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);
        Assert.True(pageOne.IsSuccess);
        Assert.NotNull(pageOne.Value);
        Assert.Equal(3L, pageOne.Value.Count);
        Assert.Single(pageOne.Value.Departments);

        var pageTwoResponse = await AppHttpClient.GetAsync(BuildChildUrl(parent.Id.Value, page: 2, size: 1), cancellationToken);
        var pageTwo = await pageTwoResponse.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);
        Assert.True(pageTwo.IsSuccess);
        Assert.NotNull(pageTwo.Value);
        Assert.Equal(3L, pageTwo.Value.Count);
        Assert.Single(pageTwo.Value.Departments);

        var pageThreeResponse = await AppHttpClient.GetAsync(BuildChildUrl(parent.Id.Value, page: 3, size: 1), cancellationToken);
        var pageThree = await pageThreeResponse.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);
        Assert.True(pageThree.IsSuccess);
        Assert.NotNull(pageThree.Value);
        Assert.Equal(3L, pageThree.Value.Count);
        Assert.Single(pageThree.Value.Departments);

        var allChildIds = pageOne.Value.Departments
            .Concat(pageTwo.Value.Departments)
            .Concat(pageThree.Value.Departments)
            .Select(d => d.Id)
            .ToList();

        Assert.Equal(3, allChildIds.Distinct().Count());
        Assert.Contains(childA.Id.Value, allChildIds);
        Assert.Contains(childB.Id.Value, allChildIds);
        Assert.Contains(childC.Id.Value, allChildIds);
    }

    [Fact]
    public async Task GetChildDepartments_with_nonexistent_parent_should_return_empty()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var nonExistentParentId = Guid.NewGuid();

        var response = await AppHttpClient.GetAsync(BuildChildUrl(nonExistentParentId), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Departments);
        Assert.Equal(0L, result.Value.Count);
    }

    [Fact]
    public async Task GetChildDepartments_should_respect_page_size_limits()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var parent = await CreateDepartmentAsync([location.Id.Value], "parent_dept", "parentdept");

        for (int i = 0; i < 5; i++)
        {
            string childName = i switch
            {
                0 => "child_alpha",
                1 => "child_beta",
                2 => "child_gamma",
                3 => "child_delta",
                4 => "child_epsilon",
                _ => $"child_{i}"
            };
            string childIdentifier = i switch
            {
                0 => "childalpha",
                1 => "childbeta",
                2 => "childgamma",
                3 => "childdelta",
                4 => "childepsilon",
                _ => $"child{i}"
            };

            await CreateDepartmentAsync([location.Id.Value], childName, childIdentifier, parent: parent);
        }

        var response = await AppHttpClient.GetAsync(BuildChildUrl(parent.Id.Value, page: 1, size: 2), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(5L, result.Value.Count);
        Assert.Equal(2, result.Value.Departments.Count);
    }

    [Fact]
    public async Task GetChildDepartments_with_page_zero_should_fail_validation()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var parentId = Guid.NewGuid();

        var response = await AppHttpClient.GetAsync(BuildChildUrl(parentId, page: 0, size: 10), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse?>(cancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetChildDepartments_with_size_zero_should_fail_validation()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var parentId = Guid.NewGuid();

        var response = await AppHttpClient.GetAsync(BuildChildUrl(parentId, page: 1, size: 0), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse?>(cancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetChildDepartments_with_negative_page_should_fail_validation()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var parentId = Guid.NewGuid();

        var response = await AppHttpClient.GetAsync(BuildChildUrl(parentId, page: -1, size: 10), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse?>(cancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetChildDepartments_with_negative_size_should_fail_validation()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var parentId = Guid.NewGuid();

        var response = await AppHttpClient.GetAsync(BuildChildUrl(parentId, page: 1, size: -5), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse?>(cancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetChildDepartments_should_handle_deep_nested_structure()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var level1 = await CreateDepartmentAsync([location.Id.Value], "level_1", "levelone");
        var level2 = await CreateDepartmentAsync([location.Id.Value], "level_2", "leveltwo", parent: level1);
        var level3 = await CreateDepartmentAsync([location.Id.Value], "level_3", "levelthree", parent: level2);

        var level1ChildrenResponse = await AppHttpClient.GetAsync(BuildChildUrl(level1.Id.Value), cancellationToken);
        var level1Children = await level1ChildrenResponse.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);
        Assert.True(level1Children.IsSuccess);
        Assert.Single(level1Children.Value.Departments);
        Assert.Equal(level2.Id.Value, level1Children.Value.Departments[0].Id);

        var level2ChildrenResponse = await AppHttpClient.GetAsync(BuildChildUrl(level2.Id.Value), cancellationToken);
        var level2Children = await level2ChildrenResponse.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);
        Assert.True(level2Children.IsSuccess);
        Assert.Single(level2Children.Value.Departments);
        Assert.Equal(level3.Id.Value, level2Children.Value.Departments[0].Id);

        var level3ChildrenResponse = await AppHttpClient.GetAsync(BuildChildUrl(level3.Id.Value), cancellationToken);
        var level3Children = await level3ChildrenResponse.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);
        Assert.True(level3Children.IsSuccess);
        Assert.Empty(level3Children.Value.Departments);
    }

    [Fact]
    public async Task GetChildDepartments_should_return_only_active_children()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var parent = await CreateDepartmentAsync([location.Id.Value], "parent_dept", "parentdept");
        var activeChild = await CreateDepartmentAsync([location.Id.Value], "active_child", "activechild", parent: parent, isActive: true);
        var inactiveChild = await CreateDepartmentAsync([location.Id.Value], "inactive_child", "inactivechild", parent: parent, isActive: false);

        var response = await AppHttpClient.GetAsync(BuildChildUrl(parent.Id.Value), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Departments);
        Assert.Equal(activeChild.Id.Value, result.Value.Departments[0].Id);
        Assert.DoesNotContain(inactiveChild.Id.Value, result.Value.Departments.Select(d => d.Id));
    }

    private static string BuildChildUrl(Guid parentId, int page = 1, int size = 20)
        => $"/api/Departments/{parentId}/children?page={page}&size={size}";
}