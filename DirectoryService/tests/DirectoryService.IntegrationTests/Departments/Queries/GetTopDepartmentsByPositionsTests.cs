using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace DirectoryService.IntegrationTests.Departments.Queries;

public class GetTopDepartmentsByPositionsTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task GetTopDepartmentsByPositions_with_empty_database_should_succeed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var response = await AppHttpClient.GetAsync(BuildTopPositionsUrl(10), cancellationToken);
        var result = await response.HandleResponseAsync<GetTopDepartmentsByPositionsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Departments);
        Assert.Equal(0, result.Value.Count);
    }

    [Fact]
    public async Task GetTopDepartmentsByPositions_without_department_positions_should_return_empty()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        await CreateDepartmentAsync([location.Id.Value], "only_dept", "only");

        var response = await AppHttpClient.GetAsync(BuildTopPositionsUrl(10), cancellationToken);
        var result = await response.HandleResponseAsync<GetTopDepartmentsByPositionsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Departments);
        Assert.Equal(0, result.Value.Count);
    }

    [Fact]
    public async Task GetTopDepartmentsByPositions_should_order_by_positions_count_desc_then_name_asc()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var deptLow = await CreateDepartmentAsync([location.Id.Value], "dept_b", "deptb");
        var deptMid = await CreateDepartmentAsync([location.Id.Value], "dept_a", "depta");
        var deptHigh = await CreateDepartmentAsync([location.Id.Value], "dept_c", "deptc");

        foreach (var (deptId, name) in new[]
                 {
                     (deptLow.Id.Value, "pos_low_1"),
                     (deptMid.Id.Value, "pos_mid_1"),
                     (deptMid.Id.Value, "pos_mid_2"),
                     (deptHigh.Id.Value, "pos_high_1"),
                     (deptHigh.Id.Value, "pos_high_2"),
                     (deptHigh.Id.Value, "pos_high_3"),
                 })
        {
            await CreatePositionAsync([deptId], name);
        }

        var response = await AppHttpClient.GetAsync(BuildTopPositionsUrl(10), cancellationToken);
        var result = await response.HandleResponseAsync<GetTopDepartmentsByPositionsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count);

        var ordered = result.Value.Departments.ToList();
        Assert.Equal(deptHigh.Id.Value, ordered[0].Id);
        Assert.Equal(3, ordered[0].PositionsCount);
        Assert.Equal(deptMid.Id.Value, ordered[1].Id);
        Assert.Equal(2, ordered[1].PositionsCount);
        Assert.Equal(deptLow.Id.Value, ordered[2].Id);
        Assert.Equal(1, ordered[2].PositionsCount);
    }

    [Fact]
    public async Task GetTopDepartmentsByPositions_with_equal_counts_should_order_names_ascending()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var deptZ = await CreateDepartmentAsync([location.Id.Value], "zebra", "zebrx");
        var deptA = await CreateDepartmentAsync([location.Id.Value], "alpha", "alphx");
        var deptM = await CreateDepartmentAsync([location.Id.Value], "middle", "middx");

        foreach (var (deptId, name) in new[] { (deptZ.Id.Value, "tie_z"), (deptA.Id.Value, "tie_a"), (deptM.Id.Value, "tie_m") })
        {
            await CreatePositionAsync([deptId], name);
        }

        var response = await AppHttpClient.GetAsync(BuildTopPositionsUrl(10), cancellationToken);
        var result = await response.HandleResponseAsync<GetTopDepartmentsByPositionsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count);

        var names = result.Value.Departments.Select(d => d.Name).ToList();
        Assert.Equal(["alpha", "middle", "zebra"], names);
        Assert.All(result.Value.Departments, d => Assert.Equal(1, d.PositionsCount));
    }

    [Fact]
    public async Task GetTopDepartmentsByPositions_should_respect_top_count()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var first = await CreateDepartmentAsync([location.Id.Value], "first", "first");
        var second = await CreateDepartmentAsync([location.Id.Value], "second", "second");

        foreach (var (deptId, name) in new[] { (first.Id.Value, "fst_one"), (first.Id.Value, "fst_two"), (second.Id.Value, "snd_one") })
        {
            await CreatePositionAsync([deptId], name);
        }

        var response = await AppHttpClient.GetAsync(BuildTopPositionsUrl(1), cancellationToken);
        var result = await response.HandleResponseAsync<GetTopDepartmentsByPositionsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Departments);
        Assert.Equal(1, result.Value.Count);
        Assert.Equal(first.Id.Value, result.Value.Departments[0].Id);
        Assert.Equal(2, result.Value.Departments[0].PositionsCount);
    }

    [Fact]
    public async Task GetTopDepartmentsByPositions_with_top_count_greater_than_matches_should_return_all()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var dept = await CreateDepartmentAsync([location.Id.Value], "solo", "solo");

        await CreatePositionAsync([dept.Id.Value], "solo_pos");

        var response = await AppHttpClient.GetAsync(BuildTopPositionsUrl(100), cancellationToken);
        var result = await response.HandleResponseAsync<GetTopDepartmentsByPositionsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Departments);
        Assert.Equal(1, result.Value.Count);
    }

    [Fact]
    public async Task GetTopDepartmentsByPositions_with_zero_top_count_should_fail_validation()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var response = await AppHttpClient.GetAsync(BuildTopPositionsUrl(0), cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetTopDepartmentsByPositions_with_top_count_above_limit_should_fail_validation()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var response = await AppHttpClient.GetAsync(BuildTopPositionsUrl(1001), cancellationToken);
        var result = await response.HandleResponseAsync(cancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task GetTopDepartmentsByPositions_with_max_allowed_top_count_should_succeed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var response = await AppHttpClient.GetAsync(BuildTopPositionsUrl(1000), cancellationToken);
        var result = await response.HandleResponseAsync<GetTopDepartmentsByPositionsResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    private string BuildTopPositionsUrl(int topCount)
        => $"/api/Departments/top-positions?topCount={topCount}";
}
