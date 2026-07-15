using System.Data;
using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Locations.Dtos;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsHandler(
    IValidator<GetLocationsQuery> validator,
    IDbConnectionFactory connectionFactory)
    : IQueryHandler<PagedResult<LocationDto>, GetLocationsQuery>
{
    private const string SEARCH_PARAMETER = "search";
    private const string IS_ACTIVE_PARAMETER = "is_active";
    private const string DEPARTMENT_IDS_PARAMETER = "department_ids";
    private const string OFFSET_PARAMETER = "offset";
    private const string PAGE_SIZE_PARAMETER = "page_size";

    public async Task<Result<PagedResult<LocationDto>, Errors>> Handle(GetLocationsQuery query, CancellationToken cancellationToken)
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

        int pageSize = request.Pagination!.PageSize;
        int offset = (request.Pagination!.Page - 1) * pageSize;

        parameters.Add(OFFSET_PARAMETER, offset, DbType.Int32);
        parameters.Add(PAGE_SIZE_PARAMETER, pageSize, DbType.Int32);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add($"l.name ILIKE @{SEARCH_PARAMETER}");
            parameters.Add(SEARCH_PARAMETER, $"%{request.Search}%", DbType.String);
        }

        if (request.IsActive.HasValue)
        {
            conditions.Add($"l.is_active = @{IS_ACTIVE_PARAMETER}");
            parameters.Add(IS_ACTIVE_PARAMETER, request.IsActive, DbType.Boolean);
        }

        if (request.DepartmentIds != null && request.DepartmentIds.Length != 0)
        {
            conditions.Add(
                $"""
                EXISTS (
                    SELECT 1
                    FROM department_locations dl_filter
                    WHERE dl_filter.location_id = l.id
                      AND dl_filter.department_id = ANY(@{DEPARTMENT_IDS_PARAMETER})
                )
                """);
            parameters.Add(DEPARTMENT_IDS_PARAMETER, request.DepartmentIds);
        }

        conditions.Add("l.deleted_at IS NULL");

        string whereClause = conditions.Any()
            ? $"WHERE {string.Join(" AND ", conditions)}"
            : string.Empty;

        string orderByField = query.Request.SortBy?.ToLower() switch
        {
            "name" => "name",
            "country" => "country",
            "created_at" => "created_at",
            _ => "name",
        };

        string orderByClauseFormat = string.Equals(query.Request.SortDirection, "asc")
            ? $"ORDER BY {{0}}.{orderByField} ASC"
            : $"ORDER BY {{0}}.{orderByField} DESC";

        long? totalCount = null;
        var locationsDtoMap = new Dictionary<Guid, LocationDto>();
        await connection.QueryAsync<LocationDto, LocationAddressDto, Guid?, long, LocationDto>(
            $"""
            WITH filtered_locations AS (
                SELECT
                    l.id,
                    l.name,
                    l.created_at,
                    l.is_active,
                    l.timezone,
                    l.country,
                    l.city,
                    l.street,
                    l.apartment,
                    l.postal_code,
                    l.building_number
                FROM locations l
                {whereClause}
            ),
            paged_locations AS (
                SELECT
                    fl.*,
                    COUNT(*) OVER() AS total_count
                FROM filtered_locations fl
                {string.Format(orderByClauseFormat, "fl")}
                LIMIT @{PAGE_SIZE_PARAMETER} OFFSET @{OFFSET_PARAMETER}
            )
            SELECT
                pl.id,
                pl.name,
                pl.created_at,
                pl.is_active,
                pl.timezone,
                
                pl.country,
                pl.city,
                pl.street,
                pl.apartment,
                pl.postal_code,
                pl.building_number,
                
                dl.department_id,
                pl.total_count
            FROM paged_locations pl
            LEFT JOIN department_locations dl ON pl.id = dl.location_id
            {string.Format(orderByClauseFormat, "pl")}
            """,
            param: parameters,
            splitOn: "country,department_id,total_count",
            map: (locationDto, addressDto, departmentId, count) =>
            {
                if (locationsDtoMap.TryGetValue(locationDto.Id, out var dto))
                {
                    locationDto = dto;
                }
                else
                {
                    locationDto.Address = addressDto;
                    locationsDtoMap.Add(locationDto.Id, locationDto);
                }

                if (departmentId.HasValue)
                {
                    locationDto.DepartmentIds.Add(departmentId.Value);
                }

                totalCount ??= count;
                return locationDto;
            });

        return new PagedResult<LocationDto>(
            locationsDtoMap.Values.ToList(),
            totalCount ?? 0,
            request.Pagination.Page,
            request.Pagination.PageSize);
    }
}