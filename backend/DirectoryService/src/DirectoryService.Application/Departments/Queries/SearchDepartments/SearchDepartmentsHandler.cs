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
    : IQueryHandler<PagedResult<AncestorDepartmentDto>, SearchDepartmentsQuery>
{
    private const string LIMIT_PARAMETER = "limit";
    private const string OFFSET_PARAMETER = "offset";
    private const string SEARCH_PARAMETER = "search";

    public async Task<Result<PagedResult<AncestorDepartmentDto>, Errors>> Handle(SearchDepartmentsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var request = query.Request;

        var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);

        int offset = (request.Page - 1) * request.PageSize;

        var parameters = new DynamicParameters();
        parameters.Add(LIMIT_PARAMETER, request.PageSize, DbType.Int32);
        parameters.Add(OFFSET_PARAMETER, offset, DbType.Int32);
        parameters.Add(SEARCH_PARAMETER, $"%{request.Name}%", DbType.String);

        long? totalCount = null;

        var departmentDtos = (await connection.QueryAsync<AncestorDepartmentDto, long, AncestorDepartmentDto>(
            $"""
            WITH matched AS (
                SELECT
                    d.id,
                    d.parent_id,
                    d.name,
                    d.identifier,
                    d.depth,
                    d.path
                FROM departments d
                WHERE d.is_active = TRUE
                  AND d.name ILIKE @{SEARCH_PARAMETER}
            )
            SELECT
                *,
                COUNT(*) OVER() as total_count
            FROM matched
            ORDER BY depth, name
            LIMIT @{LIMIT_PARAMETER}
            OFFSET @{OFFSET_PARAMETER}
            """,
            param: parameters,
            splitOn: "total_count",
            map: (departmentDto, count) =>
            {
                totalCount ??= count;
                return departmentDto;
            }))
            .ToList();

        return new PagedResult<AncestorDepartmentDto>(
            departmentDtos,
            totalCount ?? 0,
            request.Page,
            request.PageSize);
    }
}