using System.Data;
using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Departments.Dtos;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Departments.Queries.SearchDepartments;

public class SearchDepartmentsHandler(
    IValidator<SearchDepartmentsQuery> validator,
    IDbConnectionFactory connectionFactory)
    : IQueryHandler<PagedResult<SearchDepartmentDto>, SearchDepartmentsQuery>
{
    private const string LIMIT_PARAMETER = "limit";
    private const string OFFSET_PARAMETER = "offset";
    private const string SEARCH_PARAMETER = "search";
    private const string IS_ACTIVE_PARAMETER = "is_active";
    private const string PARENT_ID_PARAMETER = "parent_id";
    private const string LOCATION_IDS_PARAMETER = "location_ids";
    private const string EXCLUDE_IDS_PARAMETER = "exclude_ids";

    public async Task<Result<PagedResult<SearchDepartmentDto>, Errors>> Handle(
        SearchDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var request = query.Request;

        var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);

        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        int pageSize = request.PageSize;
        int offset = (request.Page - 1) * pageSize;

        parameters.Add(LIMIT_PARAMETER, request.PageSize, DbType.Int32);
        parameters.Add(OFFSET_PARAMETER, offset, DbType.Int32);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add($"(d.name ILIKE @{SEARCH_PARAMETER} OR d.identifier ILIKE @{SEARCH_PARAMETER})");
            parameters.Add(SEARCH_PARAMETER, $"%{request.Search}%", DbType.String);
        }

        if (request.IsActive.HasValue)
        {
            conditions.Add($"d.is_active = @{IS_ACTIVE_PARAMETER}");
            parameters.Add(IS_ACTIVE_PARAMETER, request.IsActive, DbType.Boolean);
        }

        if (request.ParentId.HasValue)
        {
            conditions.Add($"d.parent_id = @{PARENT_ID_PARAMETER}");
            parameters.Add(PARENT_ID_PARAMETER, request.ParentId, DbType.Guid);
        }

        if (request.LocationIds != null && request.LocationIds.Any())
        {
            conditions.Add(
                $"""
                 EXISTS (
                     SELECT 1
                     FROM department_locations dl
                     WHERE dl.department_id = d.id
                       AND dl.location_id = ANY(@{LOCATION_IDS_PARAMETER})
                 )
                 """);
            parameters.Add(LOCATION_IDS_PARAMETER, request.LocationIds);
        }

        if (request.ExcludeIds != null && request.ExcludeIds.Any())
        {
            conditions.Add($"d.id != ALL(@{EXCLUDE_IDS_PARAMETER})");
            parameters.Add(EXCLUDE_IDS_PARAMETER, request.ExcludeIds);
        }

        conditions.Add("d.deleted_at IS NULL");

        string whereClause = conditions.Any()
            ? $"WHERE {string.Join(" AND ", conditions)}"
            : string.Empty;

        string orderByField = query.Request.SortBy.ToLower() switch
        {
            "name" => "name",
            "identifier" => "identifier",
            "created_at" => "created_at",
            _ => "name",
        };

        string orderByClauseFormat = string.Equals(query.Request.SortDirection, "asc")
            ? $"ORDER BY {{0}}.{orderByField} ASC"
            : $"ORDER BY {{0}}.{orderByField} DESC";

        long? totalCount = null;
        var departmentsDtoMap = new Dictionary<Guid, SearchDepartmentDto>();
        await connection.QueryAsync<SearchDepartmentDto, long, SearchDepartmentDto>(
            $"""
             SELECT
                 d.id,
                 d.name, 
                 d.identifier,
                 d.path,
                 d.parent_id,
                 d.depth,
                 d.is_active,
                 d.created_at,
                 d.updated_at,
                 d.deleted_at,
                 COUNT(*) OVER() AS total_count
             FROM departments d
             {whereClause}
             {string.Format(orderByClauseFormat, "d")}
             LIMIT @{LIMIT_PARAMETER} OFFSET @{OFFSET_PARAMETER}
             """,
            param: parameters,
            splitOn: "total_count",
            map: (departmentDto, count) =>
            {
                departmentsDtoMap.TryAdd(departmentDto.Id, departmentDto);
                totalCount ??= count;
                return departmentDto;
            });

        return new PagedResult<SearchDepartmentDto>(
            departmentsDtoMap.Values.ToList(),
            totalCount ?? 0,
            request.Page,
            request.PageSize);
    }
}