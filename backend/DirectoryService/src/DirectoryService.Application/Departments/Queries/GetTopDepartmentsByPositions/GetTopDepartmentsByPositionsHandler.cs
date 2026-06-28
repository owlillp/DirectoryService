using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Contracts.Departments.Dtos;
using DirectoryService.Contracts.Departments.Responses;
using FluentValidation;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Departments.Queries.GetTopDepartmentsByPositions;

public class GetTopDepartmentsByPositionsHandler(
    IValidator<GetTopDepartmentsByPositionQuery> validator,
    IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetTopDepartmentsByPositionsResponse, GetTopDepartmentsByPositionQuery>
{
    public async Task<Result<GetTopDepartmentsByPositionsResponse, Errors>> Handle(
        GetTopDepartmentsByPositionQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);

        var topDepartmentDtos =
            (await connection.QueryAsync<TopDepartmentByPositionsDto, int, TopDepartmentByPositionsDto>(
                $"""
                SELECT
                    d.id,
                    d.name,
                    d.path,
                    COUNT(dp.position_id)::int AS positions_count
                FROM departments d 
                LEFT JOIN department_positions dp ON dp.department_id = d.id
                JOIN positions p ON dp.position_id = p.id
                WHERE d.is_active = TRUE
                    AND p.is_active = TRUE
                GROUP BY d.id, d.name
                ORDER BY positions_count DESC, d.name ASC
                LIMIT {query.TopCount}
                """,
                splitOn: "positions_count",
                map: (topDepartmentDto, positionsCount)
                    => topDepartmentDto with { PositionsCount = positionsCount }))
            .ToList();

        return new GetTopDepartmentsByPositionsResponse(topDepartmentDtos, topDepartmentDtos.Count);
    }
}