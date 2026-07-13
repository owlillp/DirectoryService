using System.Data;
using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Positions.Dtos;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Positions.Queries.GetPositionsInfinite;

public class GetPositionsInfiniteHandler(
    IValidator<GetPositionsInfiniteQuery> validator,
    IDbConnectionFactory connectionFactory
    ) : IQueryHandler<InfinitePagedResult<PositionDto>, GetPositionsInfiniteQuery>
{
    private const string SEARCH_PARAMETER = "search";
    private const string IS_ACTIVE_PARAMETER = "is_active";
    private const string LIMIT_PARAMETER = "limit";
    private const string CURSOR_ID_PARAMETER = "cursor_id";
    private const string CURSOR_VALUE_PARAMETER = "cursor_value";

    public async Task<Result<InfinitePagedResult<PositionDto>, Errors>> Handle(GetPositionsInfiniteQuery infiniteQuery, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(infiniteQuery, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var request = infiniteQuery.InfiniteRequest;

        var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);

        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add($"p.name ILIKE @{SEARCH_PARAMETER}");
            parameters.Add(SEARCH_PARAMETER, $"%{request.Search}%", DbType.String);
        }

        if (request.IsActive.HasValue)
        {
            conditions.Add($"p.is_active = @{IS_ACTIVE_PARAMETER}");
            parameters.Add(IS_ACTIVE_PARAMETER, request.IsActive, DbType.Boolean);
        }

        parameters.Add(LIMIT_PARAMETER, request.InfiniteRequest.Limit + 1, DbType.Int32);

        string orderByField = infiniteQuery.InfiniteRequest.SortBy?.ToLower() switch
        {
            "name" => "name",
            "created_at" => "created_at",
            _ => "name",
        };

        if (request.InfiniteRequest.Cursor != null)
        {
            parameters.Add(CURSOR_ID_PARAMETER, request.InfiniteRequest.Cursor.Id, DbType.Guid);
            switch(orderByField)
            {
                case "created_at":
                    parameters.Add(CURSOR_VALUE_PARAMETER, DateTime.Parse(request.InfiniteRequest.Cursor.Value!), DbType.DateTime);
                    break;
                default:
                    parameters.Add(CURSOR_VALUE_PARAMETER, request.InfiniteRequest.Cursor.Value, DbType.String);
                    break;
            }

            conditions.Add(string.Equals(infiniteQuery.InfiniteRequest.SortDirection, "asc")
                ? $"(p.{orderByField}, p.id) > (@{CURSOR_VALUE_PARAMETER}, @{CURSOR_ID_PARAMETER})"
                : $"(p.{orderByField}, p.id) < (@{CURSOR_VALUE_PARAMETER}, @{CURSOR_ID_PARAMETER})");
        }

        string whereClause = conditions.Any()
            ? $"WHERE {string.Join(" AND ", conditions)}"
            : string.Empty;

        string orderByClauseFormat = string.Equals(infiniteQuery.InfiniteRequest.SortDirection, "asc")
            ? $"ORDER BY {{0}}.{orderByField} ASC, {{0}}.id ASC"
            : $"ORDER BY {{0}}.{orderByField} DESC, {{0}}.id DESC";

        var positionsDtoMap = new Dictionary<Guid, PositionDto>();
        await connection.QueryAsync<PositionDto, Guid?, PositionDto>(
            $"""
              WITH filtered_positions AS (
                 SELECT
                     p.id,
                     p.name,
                     p.description,
                     p.is_active,
                     p.created_at
                 FROM positions p
                 {whereClause}
                 {string.Format(orderByClauseFormat, "p")}
                 LIMIT @{LIMIT_PARAMETER}
              )
              SELECT
                  fp.id,
                  fp.name,
                  fp.description,
                  fp.is_active,
                  fp.created_at,
                  dp.department_id
              FROM filtered_positions fp
              LEFT JOIN department_positions dp
                  ON dp.position_id = fp.id
              {string.Format(orderByClauseFormat, "fp")}
              """,
            param: parameters,
            splitOn: "department_id",
            map: (positionDto, departmentId) =>
            {
                if (positionsDtoMap.TryGetValue(positionDto.Id, out var existing))
                {
                    positionDto = existing;
                }
                else
                {
                    positionsDtoMap.Add(positionDto.Id, positionDto);
                }

                if (departmentId.HasValue)
                {
                    positionDto.DepartmentIds.Add(departmentId.Value);
                }

                return positionDto;
            });

        bool hasNextPage = positionsDtoMap.Count > request.InfiniteRequest.Limit;
        var records = positionsDtoMap.Values.Take(request.InfiniteRequest.Limit).ToList();
        Cursor? nextCursor = hasNextPage
            ? GetNextCursor(records.Last(), orderByField)
            : null;

        return new InfinitePagedResult<PositionDto>(records, nextCursor, hasNextPage);
    }

    private Cursor GetNextCursor(PositionDto lastDto, string sortField) =>
        new (
            lastDto.Id,
            sortField switch
            {
                "created_at" => lastDto.CreatedAt.ToString("O"),
                _ => lastDto.Name,
            });
}
