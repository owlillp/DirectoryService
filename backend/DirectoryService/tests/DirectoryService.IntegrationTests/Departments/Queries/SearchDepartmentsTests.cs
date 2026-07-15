using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Departments.Dtos;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace DirectoryService.IntegrationTests.Departments.Queries;

public class SearchDepartmentsTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task SearchDepartments_should_return_all_active_departments_when_no_filters()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var deptA = await CreateDepartmentAsync([location.Id.Value], name: "alpha", identifier: "alpha");
        var deptB = await CreateDepartmentAsync([location.Id.Value], name: "beta", identifier: "beta");

        var url = BuildSearchUrl(page: 1, pageSize: 50);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.TotalCount >= 2);

        var ids = result.Value.Records.Select(d => d.Id).ToHashSet();
        Assert.Contains(deptA.Id.Value, ids);
        Assert.Contains(deptB.Id.Value, ids);
    }

    [Fact]
    public async Task SearchDepartments_should_filter_by_search_term()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var matchingDept = await CreateDepartmentAsync([location.Id.Value], name: "unique_search_term", identifier: "unique");
        var nonMatchingDept = await CreateDepartmentAsync([location.Id.Value], name: "other", identifier: "other");

        var url = BuildSearchUrl(search: "unique", page: 1, pageSize: 50);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.TotalCount);

        var dto = Assert.Single(result.Value.Records);
        Assert.Equal(matchingDept.Id.Value, dto.Id);
        Assert.Equal("unique_search_term", dto.Name);
        Assert.Equal("unique", dto.Identifier);
        Assert.DoesNotContain(nonMatchingDept.Id.Value, result.Value.Records.Select(d => d.Id));
    }

    [Fact]
    public async Task SearchDepartments_should_filter_by_is_active()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var activeDept = await CreateDepartmentAsync([location.Id.Value], name: "active_dept", identifier: "active", isActive: true);
        var inactiveDept = await CreateDepartmentAsync([location.Id.Value], name: "inactive_dept", identifier: "inactive", isActive: false);

        var url = BuildSearchUrl(isActive: true, page: 1, pageSize: 50);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.All(result.Value.Records, d => Assert.True(d.IsActive));
        Assert.Contains(activeDept.Id.Value, result.Value.Records.Select(d => d.Id));
        Assert.DoesNotContain(inactiveDept.Id.Value, result.Value.Records.Select(d => d.Id));
    }

    [Fact]
    public async Task SearchDepartments_should_filter_by_parent_id()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var parent = await CreateDepartmentAsync([location.Id.Value], name: "parent", identifier: "parent");
        var child1 = await CreateDepartmentAsync([location.Id.Value], name: "child_one", identifier: "childone", parent: parent);
        var child2 = await CreateDepartmentAsync([location.Id.Value], name: "child_two", identifier: "childtwo", parent: parent);
        var unrelated = await CreateDepartmentAsync([location.Id.Value], name: "unrelated", identifier: "unrelated");

        var url = BuildSearchUrl(parentId: parent.Id.Value, page: 1, pageSize: 50);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.TotalCount);

        var ids = result.Value.Records.Select(d => d.Id).ToHashSet();
        Assert.Contains(child1.Id.Value, ids);
        Assert.Contains(child2.Id.Value, ids);
        Assert.DoesNotContain(unrelated.Id.Value, ids);
        Assert.DoesNotContain(parent.Id.Value, ids);
        Assert.All(result.Value.Records, d => Assert.Equal(parent.Id.Value, d.ParentId));
    }

    [Fact]
    public async Task SearchDepartments_should_filter_by_location_ids()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var locationA = await CreateLocationAsync("loc_a", "country_a");
        var locationB = await CreateLocationAsync("loc_b", "country_b");
        var deptWithLocA = await CreateDepartmentAsync([locationA.Id.Value], name: "dept_a", identifier: "depta");
        var deptWithLocB = await CreateDepartmentAsync([locationB.Id.Value], name: "dept_b", identifier: "deptb");

        var url = BuildSearchUrl(locationIds: [locationA.Id.Value], page: 1, pageSize: 50);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Records);
        Assert.Equal(deptWithLocA.Id.Value, result.Value.Records[0].Id);
        Assert.DoesNotContain(deptWithLocB.Id.Value, result.Value.Records.Select(d => d.Id));
    }

    [Fact]
    public async Task SearchDepartments_should_filter_by_multiple_location_ids()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var locationA = await CreateLocationAsync("loc_a", "country_a");
        var locationB = await CreateLocationAsync("loc_b", "country_b");
        var locationC = await CreateLocationAsync("loc_c", "country_c");
        var deptWithA = await CreateDepartmentAsync([locationA.Id.Value], name: "dept_a", identifier: "depta");
        var deptWithB = await CreateDepartmentAsync([locationB.Id.Value], name: "dept_b", identifier: "deptb");
        var deptWithC = await CreateDepartmentAsync([locationC.Id.Value], name: "dept_c", identifier: "deptc");

        var url = BuildSearchUrl(locationIds: [locationA.Id.Value, locationB.Id.Value], page: 1, pageSize: 50);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.TotalCount);

        var ids = result.Value.Records.Select(d => d.Id).ToHashSet();
        Assert.Contains(deptWithA.Id.Value, ids);
        Assert.Contains(deptWithB.Id.Value, ids);
        Assert.DoesNotContain(deptWithC.Id.Value, ids);
    }

    [Fact]
    public async Task SearchDepartments_should_filter_by_exclude_ids()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var deptToKeep = await CreateDepartmentAsync([location.Id.Value], name: "keep", identifier: "keep");
        var deptToExclude = await CreateDepartmentAsync([location.Id.Value], name: "exclude", identifier: "exclude");

        var url = BuildSearchUrl(excludeIds: [deptToExclude.Id.Value], page: 1, pageSize: 50);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Contains(deptToKeep.Id.Value, result.Value.Records.Select(d => d.Id));
        Assert.DoesNotContain(deptToExclude.Id.Value, result.Value.Records.Select(d => d.Id));
    }

    [Fact]
    public async Task SearchDepartments_should_combine_multiple_filters()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var parent = await CreateDepartmentAsync([location.Id.Value], name: "parentdept", identifier: "parent", isActive: true);

        var matchingDept = await CreateDepartmentAsync(
            [location.Id.Value],
            name: "targetdept",
            identifier: "target",
            parent: parent,
            isActive: true);

        await CreateDepartmentAsync(
            [location.Id.Value],
            name: "otherdept",
            identifier: "other",
            parent: parent,
            isActive: true);

        await CreateDepartmentAsync(
            [location.Id.Value],
            name: "targetdeptt",
            identifier: "targetx",
            parent: parent,
            isActive: false);

        string url = BuildSearchUrl(
            search: "targetdept",
            isActive: true,
            parentId: parent.Id.Value,
            page: 1,
            pageSize: 50);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Records);
        Assert.Equal(matchingDept.Id.Value, result.Value.Records[0].Id);
    }

    [Fact]
    public async Task SearchDepartments_should_paginate_and_report_total_count()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        string[] deptNames = ["aaa", "bbb", "ccc", "ddd", "eee"];
        string[] deptIdents = ["aaa", "bbb", "ccc", "ddd", "eee"];
        for (int i = 0; i < 5; i++)
        {
            await CreateDepartmentAsync([location.Id.Value], name: deptNames[i], identifier: deptIdents[i]);
        }

        var pageOneUrl = BuildSearchUrl(page: 1, pageSize: 2);
        var pageThreeUrl = BuildSearchUrl(page: 3, pageSize: 2);

        // act
        var pageOneResponse = await AppHttpClient.GetAsync(pageOneUrl, cancellationToken);
        var pageOneResult = await pageOneResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        var pageThreeResponse = await AppHttpClient.GetAsync(pageThreeUrl, cancellationToken);
        var pageThreeResult = await pageThreeResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(pageOneResult.IsSuccess);
        Assert.True(pageThreeResult.IsSuccess);

        Assert.Equal(5, pageOneResult.Value!.TotalCount);
        Assert.Equal(2, pageOneResult.Value.Records.Count);

        Assert.Equal(5, pageThreeResult.Value!.TotalCount);
        Assert.Single(pageThreeResult.Value.Records);
    }

    [Fact]
    public async Task SearchDepartments_should_sort_by_name_ascending_by_default()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        await CreateDepartmentAsync([location.Id.Value], name: "beta", identifier: "beta");
        await CreateDepartmentAsync([location.Id.Value], name: "alpha", identifier: "alpha");

        string url = BuildSearchUrl(page: 1, pageSize: 50, sortBy: "name", sortDirection: "asc");

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var filteredRecords = result.Value.Records
            .Where(d => d.Name is "alpha" or "beta")
            .ToList();

        Assert.Equal(2, filteredRecords.Count);
        Assert.Equal("alpha", filteredRecords[0].Name);
        Assert.Equal("beta", filteredRecords[1].Name);
    }

    [Fact]
    public async Task SearchDepartments_should_sort_by_name_descending()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        await CreateDepartmentAsync([location.Id.Value], name: "alpha", identifier: "alpha");
        await CreateDepartmentAsync([location.Id.Value], name: "beta", identifier: "beta");

        string url = BuildSearchUrl(page: 1, pageSize: 50, sortBy: "name", sortDirection: "desc");

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var filteredRecords = result.Value.Records
            .Where(d => d.Name is "alpha" or "beta")
            .ToList();

        Assert.Equal(2, filteredRecords.Count);
        Assert.Equal("beta", filteredRecords[0].Name);
        Assert.Equal("alpha", filteredRecords[1].Name);
    }

    [Fact]
    public async Task SearchDepartments_should_sort_by_created_at()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        await CreateDepartmentAsync([location.Id.Value], name: "first_dept", identifier: "first");
        await Task.Delay(50, cancellationToken);
        await CreateDepartmentAsync([location.Id.Value], name: "second_dept", identifier: "second");

        string url = BuildSearchUrl(page: 1, pageSize: 50, sortBy: "created_at", sortDirection: "desc");

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var filteredRecords = result.Value.Records
            .Where(d => d.Name is "first_dept" or "second_dept")
            .ToList();

        Assert.Equal(2, filteredRecords.Count);
        Assert.Contains(filteredRecords[0].Name, new[] { "second_dept" });
        Assert.Contains(filteredRecords[1].Name, new[] { "first_dept" });
        Assert.True(filteredRecords[0].CreatedAt >= filteredRecords[1].CreatedAt);
    }

    [Fact]
    public async Task SearchDepartments_should_return_empty_when_no_match()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        await CreateDepartmentAsync([location.Id.Value], name: "something", identifier: "something");

        var url = BuildSearchUrl(search: "nonexistent_value_xyz", page: 1, pageSize: 10);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Records);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task SearchDepartments_with_empty_database_should_return_empty()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var url = BuildSearchUrl(page: 1, pageSize: 10);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Records);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task SearchDepartments_should_exclude_soft_deleted_departments()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var activeDept = await CreateDepartmentAsync([location.Id.Value], name: "active_dept", identifier: "active");
        var deptToDelete = await CreateDepartmentAsync([location.Id.Value], name: "to_delete", identifier: "todelete");

        var deleteResponse = await AppHttpClient.DeleteAsync($"/api/Departments/{deptToDelete.Id.Value}", cancellationToken);
        Assert.True((await deleteResponse.HandleResponseAsync(cancellationToken)).IsSuccess);

        var url = BuildSearchUrl(page: 1, pageSize: 50);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Contains(activeDept.Id.Value, result.Value.Records.Select(d => d.Id));
        Assert.DoesNotContain(deptToDelete.Id.Value, result.Value.Records.Select(d => d.Id));
    }

    [Fact]
    public async Task SearchDepartments_with_identifier_search_term()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var deptByIdentifier = await CreateDepartmentAsync([location.Id.Value], name: "some_name", identifier: "customident");
        await CreateDepartmentAsync([location.Id.Value], name: "other_name", identifier: "otherident");

        string url = BuildSearchUrl(search: "custom", page: 1, pageSize: 50);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Records);
        Assert.Equal(deptByIdentifier.Id.Value, result.Value.Records[0].Id);
    }

    [Fact]
    public async Task SearchDepartments_should_return_correct_field_values()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var parent = await CreateDepartmentAsync([location.Id.Value], name: "parent_test", identifier: "parenttest");
        var child = await CreateDepartmentAsync(
            [location.Id.Value],
            name: "child_test",
            identifier: "childtest",
            parent: parent);

        var url = BuildSearchUrl(search: "child_test", page: 1, pageSize: 10);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var dto = Assert.Single(result.Value.Records);
        Assert.Equal(child.Id.Value, dto.Id);
        Assert.Equal("child_test", dto.Name);
        Assert.Equal("childtest", dto.Identifier);
        Assert.NotNull(dto.ParentId);
        Assert.Equal(parent.Id.Value, dto.ParentId);
        Assert.Equal(1, dto.Depth);
        Assert.True(dto.IsActive);
        Assert.NotEqual(default, dto.CreatedAt);
        Assert.NotEqual(default, dto.UpdatedAt);
    }

    [Fact]
    public async Task SearchDepartments_with_empty_parent_id_should_fail_validation()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var url = BuildSearchUrl(parentId: Guid.Empty, page: 1, pageSize: 10);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task SearchDepartments_with_page_zero_should_fail_validation()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var url = BuildSearchUrl(page: 0, pageSize: 10);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task SearchDepartments_with_negative_page_should_fail_validation()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var url = BuildSearchUrl(page: -1, pageSize: 10);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task SearchDepartments_with_page_size_exceeding_max_should_fail_validation()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var url = BuildSearchUrl(page: 1, pageSize: 100);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task SearchDepartments_with_combined_exclude_ids_and_location_ids()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var locationA = await CreateLocationAsync("loc_a", "country_a");
        var locationB = await CreateLocationAsync("loc_b", "country_b");
        var deptA = await CreateDepartmentAsync([locationA.Id.Value], name: "dept_a", identifier: "depta");
        var deptB = await CreateDepartmentAsync([locationB.Id.Value], name: "dept_b", identifier: "deptb");
        var deptC = await CreateDepartmentAsync([locationA.Id.Value], name: "dept_c", identifier: "deptc");

        var url = BuildSearchUrl(
            locationIds: [locationA.Id.Value],
            excludeIds: [deptA.Id.Value],
            page: 1,
            pageSize: 50);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Records);
        Assert.Equal(deptC.Id.Value, result.Value.Records[0].Id);
        Assert.DoesNotContain(deptA.Id.Value, result.Value.Records.Select(d => d.Id));
        Assert.DoesNotContain(deptB.Id.Value, result.Value.Records.Select(d => d.Id));
    }

    [Fact]
    public async Task SearchDepartments_should_return_all_pages_across_pagination()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var createdIds = new List<Guid>();
        string[] pageNames = ["alpha", "bravo", "charlie", "delta", "echo"];
        for (int i = 0; i < 5; i++)
        {
            var dept = await CreateDepartmentAsync([location.Id.Value], name: pageNames[i], identifier: pageNames[i]);
            createdIds.Add(dept.Id.Value);
        }

        var allFoundIds = new List<Guid>();

        // act
        for (int page = 1; page <= 3; page++)
        {
            var url = BuildSearchUrl(page: page, pageSize: 2);
            var response = await AppHttpClient.GetAsync(url, cancellationToken);
            var result = await response.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(5, result.Value.TotalCount);

            allFoundIds.AddRange(result.Value.Records.Select(d => d.Id));
        }

        // assert
        Assert.Equal(5, allFoundIds.Distinct().Count());
        foreach (var id in createdIds)
        {
            Assert.Contains(id, allFoundIds);
        }
    }

    [Fact]
    public async Task SearchDepartments_should_search_by_partial_name_using_ilike()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var dept = await CreateDepartmentAsync([location.Id.Value], name: "departmentspecial", identifier: "special");

        var url = BuildSearchUrl(search: "DEPARTMENT", page: 1, pageSize: 10);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Records);
        Assert.Equal(dept.Id.Value, result.Value.Records[0].Id);
    }

    [Fact]
    public async Task SearchDepartments_should_return_root_departments_when_parent_id_is_not_specified()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var rootDept = await CreateDepartmentAsync([location.Id.Value], name: "root", identifier: "root");
        var childDept = await CreateDepartmentAsync([location.Id.Value], name: "child", identifier: "child", parent: rootDept);

        // Without parentId filter, both should appear
        var url = BuildSearchUrl(page: 1, pageSize: 50);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.TotalCount >= 2);

        var ids = result.Value.Records.Select(d => d.Id).ToHashSet();
        Assert.Contains(rootDept.Id.Value, ids);
        Assert.Contains(childDept.Id.Value, ids);
    }

    [Fact]
    public async Task SearchDepartments_should_sort_by_identifier()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        await CreateDepartmentAsync([location.Id.Value], name: "zzz", identifier: "bbb");
        await CreateDepartmentAsync([location.Id.Value], name: "aaa", identifier: "aaa");

        string url = BuildSearchUrl(page: 1, pageSize: 50, sortBy: "identifier", sortDirection: "asc");

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<SearchDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var filteredRecords = result.Value.Records
            .Where(d => d.Identifier is "aaa" or "bbb")
            .ToList();

        Assert.Equal(2, filteredRecords.Count);
        Assert.Equal("aaa", filteredRecords[0].Identifier);
        Assert.Equal("bbb", filteredRecords[1].Identifier);
    }

    private static string BuildSearchUrl(
        string? search = null,
        bool? isActive = null,
        Guid? parentId = null,
        Guid[]? locationIds = null,
        Guid[]? excludeIds = null,
        string? sortBy = null,
        string? sortDirection = null,
        int page = 1,
        int pageSize = 10)
    {
        var parts = new List<string>
        {
            $"Page={page}",
            $"PageSize={pageSize}",
        };

        if (search != null)
        {
            parts.Add($"Search={Uri.EscapeDataString(search)}");
        }

        if (isActive.HasValue)
        {
            parts.Add($"IsActive={isActive.Value.ToString().ToLowerInvariant()}");
        }

        if (parentId.HasValue)
        {
            parts.Add($"ParentId={parentId.Value}");
        }

        if (locationIds != null)
        {
            foreach (var id in locationIds)
            {
                parts.Add($"LocationIds={id}");
            }
        }

        if (excludeIds != null)
        {
            foreach (var id in excludeIds)
            {
                parts.Add($"ExcludeIds={id}");
            }
        }

        if (sortBy != null)
        {
            parts.Add($"SortBy={sortBy}");
        }

        if (sortDirection != null)
        {
            parts.Add($"SortDirection={sortDirection}");
        }

        return "/api/Departments?" + string.Join("&", parts);
    }
}