using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Departments;

public class GetRootDepartmentsTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task GetRootDepartments_with_empty_database_should_succeed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var response = await AppHttpClient.GetAsync(BuildRootsUrl(), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Departments);
        Assert.Equal(0L, result.Value.Count);
    }

    [Fact]
    public async Task GetRootDepartments_should_return_single_root_without_children()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var root = await CreateDepartmentAsync([location.Id.Value], "root_only", "rootonly");

        var response = await AppHttpClient.GetAsync(BuildRootsUrl(page: 1, size: 10, prefetch: 5), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1L, result.Value.Count);

        var dto = Assert.Single(result.Value.Departments);
        Assert.Equal(root.Id.Value, dto.Id);
        Assert.Equal("root_only", dto.Name);
        Assert.Null(dto.ParentId);
        Assert.Empty(dto.Children);
    }

    [Fact]
    public async Task GetRootDepartments_should_attach_prefetched_children_to_root()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var root = await CreateDepartmentAsync([location.Id.Value], "root_parent", "rootparent");
        var childA = await CreateDepartmentAsync([location.Id.Value], "child_a", "childa", parent: root);
        var childB = await CreateDepartmentAsync([location.Id.Value], "child_b", "childb", parent: root);

        var response = await AppHttpClient.GetAsync(BuildRootsUrl(page: 1, size: 10, prefetch: 10), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1L, result.Value.Count);

        var rootDto = Assert.Single(result.Value.Departments);
        Assert.Equal(root.Id.Value, rootDto.Id);
        Assert.Equal(2, rootDto.Children.Count);

        var childIds = rootDto.Children.Select(c => c.Id).ToHashSet();
        Assert.Contains(childA.Id.Value, childIds);
        Assert.Contains(childB.Id.Value, childIds);
        Assert.All(rootDto.Children, c => Assert.Equal(root.Id.Value, c.ParentId));
    }

    [Fact]
    public async Task GetRootDepartments_should_respect_prefetch_limit_for_children()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var root = await CreateDepartmentAsync([location.Id.Value], "root_many", "rootmany");
        await CreateDepartmentAsync([location.Id.Value], "child_one", "childone", parent: root);
        await CreateDepartmentAsync([location.Id.Value], "child_two", "childtwo", parent: root);
        await CreateDepartmentAsync([location.Id.Value], "child_three", "childthree", parent: root);

        var response = await AppHttpClient.GetAsync(BuildRootsUrl(page: 1, size: 10, prefetch: 2), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var rootDto = Assert.Single(result.Value.Departments);
        Assert.Equal(2, rootDto.Children.Count);
    }

    [Fact]
    public async Task GetRootDepartments_should_return_total_root_count_across_pages()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var first = await CreateDepartmentAsync([location.Id.Value], "root_first", "rootfirst");
        var second = await CreateDepartmentAsync([location.Id.Value], "root_second", "rootsecond");
        var third = await CreateDepartmentAsync([location.Id.Value], "root_third", "rootthird");

        var pageOneResponse = await AppHttpClient.GetAsync(BuildRootsUrl(page: 1, size: 1, prefetch: 0), cancellationToken);
        var pageOne = await pageOneResponse.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);
        Assert.True(pageOne.IsSuccess);
        Assert.NotNull(pageOne.Value);
        Assert.Equal(3L, pageOne.Value.Count);
        var firstPageRoot = Assert.Single(pageOne.Value.Departments);

        var pageTwoResponse = await AppHttpClient.GetAsync(BuildRootsUrl(page: 2, size: 1, prefetch: 0), cancellationToken);
        var pageTwo = await pageTwoResponse.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);
        Assert.True(pageTwo.IsSuccess);
        Assert.NotNull(pageTwo.Value);
        Assert.Equal(3L, pageTwo.Value.Count);
        var secondPageRoot = Assert.Single(pageTwo.Value.Departments);

        var pageThreeResponse = await AppHttpClient.GetAsync(BuildRootsUrl(page: 3, size: 1, prefetch: 0), cancellationToken);
        var pageThree = await pageThreeResponse.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);
        Assert.True(pageThree.IsSuccess);
        Assert.NotNull(pageThree.Value);
        Assert.Equal(3L, pageThree.Value.Count);
        var thirdPageRoot = Assert.Single(pageThree.Value.Departments);

        var distinctIds = new[] { firstPageRoot.Id, secondPageRoot.Id, thirdPageRoot.Id }.Distinct().ToList();
        Assert.Equal(3, distinctIds.Count);
        Assert.Contains(first.Id.Value, distinctIds);
        Assert.Contains(second.Id.Value, distinctIds);
        Assert.Contains(third.Id.Value, distinctIds);
    }

    [Fact]
    public async Task GetRootDepartments_should_ignore_non_root_departments_in_list()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var root = await CreateDepartmentAsync([location.Id.Value], "visible_root", "visibleroot");
        await CreateDepartmentAsync([location.Id.Value], "nested", "nested", parent: root);

        var response = await AppHttpClient.GetAsync(BuildRootsUrl(), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1L, result.Value.Count);
        Assert.Equal(root.Id.Value, result.Value.Departments[0].Id);
        Assert.Single(result.Value.Departments[0].Children);
    }

    [Fact]
    public async Task GetRootDepartments_with_page_zero_should_fail_validation()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var response = await AppHttpClient.GetAsync(BuildRootsUrl(page: 0, size: 10, prefetch: 3), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse?>(cancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetRootDepartments_with_size_zero_should_fail_validation()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var response = await AppHttpClient.GetAsync(BuildRootsUrl(page: 1, size: 0, prefetch: 3), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse?>(cancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetRootDepartments_with_negative_prefetch_should_fail_validation()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var response = await AppHttpClient.GetAsync(BuildRootsUrl(page: 1, size: 10, prefetch: -1), cancellationToken);
        var result = await response.HandleResponseAsync<GetRootDepartmentsResponse?>(cancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    private static string BuildRootsUrl(int page = 1, int size = 20, int prefetch = 3)
        => $"/api/Departments/roots?page={page}&size={size}&prefetch={prefetch}";
}
