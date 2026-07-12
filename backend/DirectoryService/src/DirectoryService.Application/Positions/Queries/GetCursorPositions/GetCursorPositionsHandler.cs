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

namespace DirectoryService.Application.Positions.Queries.GetCursorPositions;

public class GetCursorPositionsHandler(
    IValidator<GetCursorPositionsQuery> validator,
    IDbConnectionFactory connectionFactory
    ) : IQueryHandler<CursorPagedResult<PositionDto>, GetCursorPositionsQuery>
{
    private const string SEARCH_PARAMETER = "search";
    private const string IS_ACTIVE_PARAMETER = "is_active";
    private const string LIMIT_PARAMETER = "limit";
    private const string CURSOR_PARAMETER = "cursor";

    public async Task<Result<CursorPagedResult<PositionDto>, Errors>> Handle(GetCursorPositionsQuery query, CancellationToken cancellationToken)
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

        if (request.CursorRequest.Cursor.HasValue)
        {
            conditions.Add($"p.id > @{CURSOR_PARAMETER}");
            parameters.Add(CURSOR_PARAMETER, request.CursorRequest.Cursor, DbType.Guid);
        }

        parameters.Add(LIMIT_PARAMETER, request.CursorRequest.Limit + 1, DbType.Int32);

        string whereClause = conditions.Any()
            ? $"WHERE {string.Join(" AND ", conditions)}"
            : string.Empty;

        string orderByField = query.Request.SortBy?.ToLower() switch
        {
            "name" => "name",
            "created_at" => "created_at",
            _ => "name",
        };

        string orderByClauseFormat = string.Equals(query.Request.SortDirection, "asc")
            ? $"ORDER BY {{0}}.{orderByField} ASC"
            : $"ORDER BY {{0}}.{orderByField} DESC";

        var positionsDtoMap = new Dictionary<Guid, PositionDto>();
        await connection.QueryAsync<PositionDto, Guid?, PositionDto>(
            $"""
             SELECT
                 p.id,
                 p.name,
                 p.description,
                 p.is_active,
                 p.created_at,
                 dp.department_id
             FROM positions p
             LEFT JOIN department_positions dp ON dp.position_id = p.id
             {whereClause}
             {string.Format(orderByClauseFormat, "p")}
             LIMIT @{LIMIT_PARAMETER}
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

        bool hasNextPage = positionsDtoMap.Count > request.CursorRequest.Limit;
        var records = positionsDtoMap.Values.Take(request.CursorRequest.Limit).ToList();
        Guid? nextCursor = hasNextPage ? records.Last().Id : null;

        return new CursorPagedResult<PositionDto>(records, nextCursor, hasNextPage);
    }
}
