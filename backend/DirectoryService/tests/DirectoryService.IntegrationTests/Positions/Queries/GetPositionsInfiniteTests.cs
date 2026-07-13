using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Positions.Dtos;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace DirectoryService.IntegrationTests.Positions.Queries;

public class GetPositionsInfiniteTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task GetPositionsInfinite_empty_database_should_return_empty()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        string url = BuildUrl(limit: 10);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<InfinitePagedResult<PositionDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Records);
        Assert.Null(result.Value.NextCursor);
        Assert.False(result.Value.HasNextPage);
    }

    [Fact]
    public async Task GetPositionsInfinite_should_return_all_positions_within_limit()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        var positionA = await CreatePositionAsync([department.Id.Value], name: "alpha_pos");
        var positionB = await CreatePositionAsync([department.Id.Value], name: "beta_pos");

        string url = BuildUrl(limit: 10);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<InfinitePagedResult<PositionDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var returnedIds = result.Value.Records.Select(p => p.Id).ToHashSet();
        Assert.Contains(positionA.Id.Value, returnedIds);
        Assert.Contains(positionB.Id.Value, returnedIds);
        Assert.Null(result.Value.NextCursor);
        Assert.False(result.Value.HasNextPage);
    }

    [Fact]
    public async Task GetPositionsInfinite_should_respect_limit()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        for (int i = 0; i < 5; i++)
        {
            string posName = i switch
            {
                0 => "alpha",
                1 => "bravo",
                2 => "charlie",
                3 => "delta",
                4 => "echo",
                _ => $"pos_{i}"
            };
            await CreatePositionAsync([department.Id.Value], name: posName);
        }

        string url = BuildUrl(limit: 2);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<InfinitePagedResult<PositionDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Records.Count);
        Assert.NotNull(result.Value.NextCursor);
        Assert.True(result.Value.HasNextPage);
    }

    [Fact]
    public async Task GetPositionsInfinite_should_support_cursor_pagination()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        var positionNames = new[] { "alpha", "bravo", "charlie", "delta", "echo" };
        foreach (var name in positionNames)
        {
            await CreatePositionAsync([department.Id.Value], name: name);
        }

        string firstPageUrl = BuildUrl(limit: 2);

        // act - first page
        var firstResponse = await AppHttpClient.GetAsync(firstPageUrl, cancellationToken);
        var firstResult = await firstResponse.HandleResponseAsync<InfinitePagedResult<PositionDto>>(cancellationToken);

        // assert - first page
        Assert.True(firstResult.IsSuccess);
        Assert.NotNull(firstResult.Value);
        Assert.Equal(2, firstResult.Value.Records.Count);
        Assert.True(firstResult.Value.HasNextPage);
        Assert.NotNull(firstResult.Value.NextCursor);

        // act - second page using cursor
        var nextCursor = firstResult.Value.NextCursor;
        string secondPageUrl = BuildUrl(limit: 2, cursorId: nextCursor.Id, cursorValue: nextCursor.Value);
        var secondResponse = await AppHttpClient.GetAsync(secondPageUrl, cancellationToken);
        var secondResult = await secondResponse.HandleResponseAsync<InfinitePagedResult<PositionDto>>(cancellationToken);

        // assert - second page
        Assert.True(secondResult.IsSuccess);
        Assert.NotNull(secondResult.Value);
        Assert.Equal(2, secondResult.Value.Records.Count);
        Assert.True(secondResult.Value.HasNextPage);
        Assert.NotNull(secondResult.Value.NextCursor);

        // verify no overlap between pages
        var firstPageIds = firstResult.Value.Records.Select(p => p.Id).ToHashSet();
        var secondPageIds = secondResult.Value.Records.Select(p => p.Id).ToHashSet();
        Assert.Empty(firstPageIds.Intersect(secondPageIds));

        // act - third page (last page with 1 record)
        nextCursor = secondResult.Value.NextCursor;
        string thirdPageUrl = BuildUrl(limit: 2, cursorId: nextCursor.Id, cursorValue: nextCursor.Value);
        var thirdResponse = await AppHttpClient.GetAsync(thirdPageUrl, cancellationToken);
        var thirdResult = await thirdResponse.HandleResponseAsync<InfinitePagedResult<PositionDto>>(cancellationToken);

        // assert - third page
        Assert.True(thirdResult.IsSuccess);
        Assert.NotNull(thirdResult.Value);
        Assert.Single(thirdResult.Value.Records);
        Assert.False(thirdResult.Value.HasNextPage);
        Assert.Null(thirdResult.Value.NextCursor);
    }

    [Fact]
    public async Task GetPositionsInfinite_should_return_department_ids()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var departmentA = await CreateDepartmentAsync([location.Id.Value], name: "dept_a", identifier: "depta");
        var departmentB = await CreateDepartmentAsync([location.Id.Value], name: "dept_b", identifier: "deptb");

        var position = await CreatePositionAsync([departmentA.Id.Value, departmentB.Id.Value], name: "multi_dept_pos");

        string url = BuildUrl(limit: 10);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<InfinitePagedResult<PositionDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var dto = Assert.Single(result.Value.Records, p => p.Id == position.Id.Value);
        Assert.Equal(2, dto.DepartmentIds.Count);
        Assert.Contains(departmentA.Id.Value, dto.DepartmentIds);
        Assert.Contains(departmentB.Id.Value, dto.DepartmentIds);
    }

    [Fact]
    public async Task GetPositionsInfinite_should_filter_by_is_active()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        await CreatePositionAsync([department.Id.Value], name: "active_pos");
        var inactivePosition = await CreatePositionAsync([department.Id.Value], name: "inactive_pos");

        // soft delete the inactive position
        var deleteResponse = await AppHttpClient.DeleteAsync($"/api/Positions/{inactivePosition.Id.Value}", cancellationToken);
        Assert.True((await deleteResponse.HandleResponseAsync(cancellationToken)).IsSuccess);

        string url = BuildUrl(limit: 10, isActive: true);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<InfinitePagedResult<PositionDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.All(result.Value.Records, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task GetPositionsInfinite_should_filter_by_search()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        await CreatePositionAsync([department.Id.Value], name: "software_engineer");
        await CreatePositionAsync([department.Id.Value], name: "senior_developer");
        await CreatePositionAsync([department.Id.Value], name: "hr_manager");

        string url = BuildUrl(limit: 10, search: "software");

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<InfinitePagedResult<PositionDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var dto = Assert.Single(result.Value.Records);
        Assert.Equal("software_engineer", dto.Name);
    }

    [Fact]
    public async Task GetPositionsInfinite_should_sort_by_name_descending()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        await CreatePositionAsync([department.Id.Value], name: "aaa_position");
        await CreatePositionAsync([department.Id.Value], name: "zzz_position");

        string url = BuildUrl(limit: 10, sortBy: "name", sortDirection: "desc");

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<InfinitePagedResult<PositionDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var orderedNames = result.Value.Records
            .Where(p => p.Name is "aaa_position" or "zzz_position")
            .Select(p => p.Name)
            .ToList();

        Assert.Equal(["zzz_position", "aaa_position"], orderedNames);
    }

    [Fact]
    public async Task GetPositionsInfinite_with_zero_limit_should_fail_validation()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        string url = BuildUrl(limit: 0);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetPositionsInfinite_with_negative_limit_should_fail_validation()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        string url = BuildUrl(limit: -5);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetPositionsInfinite_with_search_exceeding_max_length_should_fail_validation()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        string search = new string('x', 1001);
        string url = BuildUrl(limit: 10, search: search);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetPositionsInfinite_deleted_position_should_not_appear_when_filtering_active()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);

        var position = await CreatePositionAsync([department.Id.Value], name: "to_delete");
        await CreatePositionAsync([department.Id.Value], name: "keep_me");

        // soft delete
        var deleteResponse = await AppHttpClient.DeleteAsync($"/api/Positions/{position.Id.Value}", cancellationToken);
        Assert.True((await deleteResponse.HandleResponseAsync(cancellationToken)).IsSuccess);

        string url = BuildUrl(limit: 10, isActive: true);

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<InfinitePagedResult<PositionDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.DoesNotContain(position.Id.Value, result.Value.Records.Select(p => p.Id));
    }

    private string BuildUrl(
        int limit = 10,
        Guid? cursorId = null,
        string? cursorValue = null,
        string? search = null,
        bool? isActive = null,
        string? sortBy = null,
        string? sortDirection = null)
    {
        var parts = new List<string>
        {
            $"InfiniteRequest.Limit={limit}",
        };

        if (cursorId.HasValue)
        {
            parts.Add($"InfiniteRequest.Cursor.Id={cursorId.Value}");
        }

        if (cursorValue != null)
        {
            parts.Add($"InfiniteRequest.Cursor.Value={Uri.EscapeDataString(cursorValue)}");
        }

        if (search != null)
        {
            parts.Add($"Search={Uri.EscapeDataString(search)}");
        }

        if (isActive.HasValue)
        {
            parts.Add($"IsActive={isActive.Value.ToString().ToLowerInvariant()}");
        }

        if (sortBy != null)
        {
            parts.Add($"SortBy={Uri.EscapeDataString(sortBy)}");
        }

        if (sortDirection != null)
        {
            parts.Add($"SortDirection={Uri.EscapeDataString(sortDirection)}");
        }

        return "/api/Positions?" + string.Join("&", parts);
    }
}