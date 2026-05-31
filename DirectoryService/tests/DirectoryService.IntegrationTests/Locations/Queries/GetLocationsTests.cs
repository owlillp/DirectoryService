using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Locations.Dtos;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Locations.Queries;

public class GetLocationsTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task GetLocations_should_return_locations_without_department_and_with_department()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var orphanedLocation = await CreateLocationAsync("orphan_loc", "orphan_country");
        var linkedLocation = await CreateLocationAsync("linked_loc", "linked_country");
        var department = await CreateDepartmentAsync([linkedLocation.Id.Value]);

        string url = BuildLocationsUrl(page: 1, pageSize: 50);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<LocationDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.TotalCount >= 2);

        var byId = result.Value.Records.ToDictionary(l => l.Id);
        Assert.True(byId.ContainsKey(orphanedLocation.Id.Value));
        Assert.True(byId.ContainsKey(linkedLocation.Id.Value));

        Assert.Empty(byId[orphanedLocation.Id.Value].DepartmentIds);
        Assert.Single(byId[linkedLocation.Id.Value].DepartmentIds);
        Assert.Contains(department.Id.Value, byId[linkedLocation.Id.Value].DepartmentIds);
    }

    [Fact]
    public async Task GetLocations_should_aggregate_department_ids_for_same_location()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync("shared_loc", "shared_country");
        var departmentFirst = await CreateDepartmentAsync([location.Id.Value], name: "dept_first", identifier: "first");
        var departmentSecond = await CreateDepartmentAsync([location.Id.Value], name: "dept_second", identifier: "second");

        string url = BuildLocationsUrl(page: 1, pageSize: 50);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<LocationDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var dto = Assert.Single(result.Value.Records, l => l.Id == location.Id.Value);
        Assert.Equal(2, dto.DepartmentIds.Count);
        Assert.Contains(departmentFirst.Id.Value, dto.DepartmentIds);
        Assert.Contains(departmentSecond.Id.Value, dto.DepartmentIds);
    }

    [Fact]
    public async Task GetLocations_should_filter_by_department_ids()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var locationOnlyDeptOne = await CreateLocationAsync("loc_dept_one", "c1");
        var locationOnlyDeptTwo = await CreateLocationAsync("loc_dept_two", "c2");
        var orphan = await CreateLocationAsync("loc_orphan", "c3");

        var departmentOne = await CreateDepartmentAsync([locationOnlyDeptOne.Id.Value], name: "filter_one", identifier: "one");
        var departmentTwo = await CreateDepartmentAsync([locationOnlyDeptTwo.Id.Value], name: "filter_two", identifier: "two");

        string url = BuildLocationsUrl(page: 1, pageSize: 50, departmentIds: [departmentOne.Id.Value]);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<LocationDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var ids = result.Value.Records.Select(l => l.Id).ToHashSet();
        Assert.Contains(locationOnlyDeptOne.Id.Value, ids);
        Assert.DoesNotContain(locationOnlyDeptTwo.Id.Value, ids);
        Assert.DoesNotContain(orphan.Id.Value, ids);

        Assert.Single(result.Value.Records);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.NotEqual(departmentOne.Id.Value, departmentTwo.Id.Value);
    }

    [Fact]
    public async Task GetLocations_should_apply_search_filter()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        await CreateLocationAsync("alpha_unique_xyz", "country_a");
        await CreateLocationAsync("beta_other", "country_b");

        string url = BuildLocationsUrl(page: 1, pageSize: 50, search: "unique_xyz");

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<LocationDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var dto = Assert.Single(result.Value.Records);
        Assert.Equal("alpha_unique_xyz", dto.Name);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetLocations_should_filter_by_is_active()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var activeLocation = await CreateLocationAsync("active_loc", "country_act", isActive: true);
        await CreateLocationAsync("inactive_loc", "country_inact", isActive: false);

        string url = BuildLocationsUrl(page: 1, pageSize: 50, isActive: true);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<LocationDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.All(result.Value.Records, l => Assert.True(l.IsActive));
        Assert.Contains(activeLocation.Id.Value, result.Value.Records.Select(l => l.Id));
    }

    [Fact]
    public async Task GetLocations_should_paginate_and_report_total_count()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        for (int i = 0; i < 5; i++)
        {
            await CreateLocationAsync($"pagination_loc_{i:D2}", "pagination_country", $"city_{i}");
        }

        string pageOneUrl = BuildLocationsUrl(page: 1, pageSize: 2);
        string pageThreeUrl = BuildLocationsUrl(page: 3, pageSize: 2);

        // act
        var pageOneResponse = await AppHttpClient.GetAsync(pageOneUrl, cancellationToken);
        var pageOneResult = await pageOneResponse.HandleResponseAsync<PagedResult<LocationDto>>(cancellationToken);

        var pageThreeResponse = await AppHttpClient.GetAsync(pageThreeUrl, cancellationToken);
        var pageThreeResult = await pageThreeResponse.HandleResponseAsync<PagedResult<LocationDto>>(cancellationToken);

        // assert
        Assert.True(pageOneResult.IsSuccess);
        Assert.True(pageThreeResult.IsSuccess);

        Assert.Equal(5, pageOneResult.Value!.TotalCount);
        Assert.Equal(2, pageOneResult.Value.Records.Count);

        Assert.Equal(5, pageThreeResult.Value!.TotalCount);
        Assert.Single(pageThreeResult.Value.Records);
    }

    [Fact]
    public async Task GetLocations_should_sort_by_name_descending_when_requested()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        await CreateLocationAsync("aaa_sort", "country_sort", "city_1");
        await CreateLocationAsync("zzz_sort", "country_sort", "city_2");

        string url = BuildLocationsUrl(page: 1, pageSize: 50, sortBy: "name", sortDirection: "desc");

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<LocationDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var orderedNames = result.Value.Records
            .Where(l => l.Name is "aaa_sort" or "zzz_sort")
            .Select(l => l.Name)
            .ToList();

        Assert.Equal(["zzz_sort", "aaa_sort"], orderedNames);
    }

    [Fact]
    public async Task GetLocations_without_pagination_should_fail_validation()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        await CreateLocationAsync();

        // act
        var httpResponse = await AppHttpClient.GetAsync(BuildLocationsUrl(includePagination: false), cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<LocationDto>?>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetLocations_with_invalid_page_should_fail_validation()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        string url = BuildLocationsUrl(page: 0, pageSize: 10);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<LocationDto>?>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetLocations_with_duplicate_department_ids_should_fail_validation()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var departmentId = Guid.NewGuid();
        string url = BuildLocationsUrl(page: 1, pageSize: 10, departmentIds: [departmentId, departmentId]);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<LocationDto>?>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetLocations_with_search_exceeding_max_length_should_fail_validation()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        string search = new string('x', 1001);
        string url = BuildLocationsUrl(page: 1, pageSize: 10, search: search);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<LocationDto>?>(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    private string BuildLocationsUrl(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        bool? isActive = null,
        IReadOnlyList<Guid>? departmentIds = null,
        string? sortBy = null,
        string? sortDirection = null,
        bool includePagination = true)
    {
        var parts = new List<string>();

        if (includePagination)
        {
            parts.Add($"Pagination.Page={page}");
            parts.Add($"Pagination.PageSize={pageSize}");
        }

        if (search != null)
        {
            parts.Add($"Search={Uri.EscapeDataString(search)}");
        }

        if (isActive.HasValue)
        {
            parts.Add($"IsActive={isActive.Value.ToString().ToLowerInvariant()}");
        }

        if (departmentIds != null)
        {
            foreach (var id in departmentIds)
            {
                parts.Add($"DepartmentIds={id}");
            }
        }

        if (sortBy != null)
        {
            parts.Add($"SortBy={Uri.EscapeDataString(sortBy)}");
        }

        if (sortDirection != null)
        {
            parts.Add($"SortDirection={Uri.EscapeDataString(sortDirection)}");
        }

        return parts.Count == 0
            ? "/api/Locations"
            : "/api/Locations?" + string.Join("&", parts);
    }
}
