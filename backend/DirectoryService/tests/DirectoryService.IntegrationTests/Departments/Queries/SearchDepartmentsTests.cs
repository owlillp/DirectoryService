using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Departments.Dtos;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.SharedKernel.Failures;
using Shared.SharedKernel.HttpCommunications;

namespace DirectoryService.IntegrationTests.Departments.Queries;

public class SearchDepartmentsTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task SearchDepartments_should_return_matching_departments()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var matchingDept = await CreateDepartmentAsync([location.Id.Value], name: "alpha_match", identifier: "alpha");
        var nonMatchingDept = await CreateDepartmentAsync([location.Id.Value], name: "beta_other", identifier: "beta");

        string url = $"/api/Departments/tree?Name={Uri.EscapeDataString("alpha")}&Page=1&PageSize=10";

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<AncestorDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.TotalCount >= 1);

        var ids = result.Value.Records.Select(d => d.Id).ToHashSet();
        Assert.Contains(matchingDept.Id.Value, ids);
        Assert.DoesNotContain(nonMatchingDept.Id.Value, ids);
    }

    [Fact]
    public async Task SearchDepartments_with_no_match_should_return_empty()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        await CreateDepartmentAsync([location.Id.Value], name: "some_dept", identifier: "some");

        string url = "/api/Departments/tree?Name=zzz_nonexistent&Page=1&PageSize=10";

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<AncestorDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Records);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task SearchDepartments_should_return_ancestor_department_fields()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var parent = await CreateDepartmentAsync([location.Id.Value], name: "parent_x", identifier: "parentx");
        var child = await CreateDepartmentAsync(
            [location.Id.Value],
            name: "child_x",
            identifier: "childx",
            parent: parent);

        string url = $"/api/Departments/tree?Name={Uri.EscapeDataString("child")}&Page=1&PageSize=10";

        // act
        var httpResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        var result = await httpResponse.HandleResponseAsync<PagedResult<AncestorDepartmentDto>>(cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var dto = Assert.Single(result.Value.Records);
        Assert.Equal(child.Id.Value, dto.Id);
        Assert.Equal("child_x", dto.Name);
        Assert.Equal("childx", dto.Identifier);
        Assert.NotNull(dto.ParentId);
        Assert.Equal(parent.Id.Value, dto.ParentId);
        Assert.Equal(1, dto.Depth);
    }

    [Fact]
    public async Task SearchDepartments_with_empty_name_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var httpResponse = await AppHttpClient.GetAsync(
            "/api/Departments/tree?Name=&Page=1&PageSize=10",
            cancellationToken);
        var result = await httpResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task SearchDepartments_with_invalid_page_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // act
        var httpResponse = await AppHttpClient.GetAsync(
            "/api/Departments/tree?Name=test&Page=0&PageSize=10",
            cancellationToken);
        var result = await httpResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
    }
}
