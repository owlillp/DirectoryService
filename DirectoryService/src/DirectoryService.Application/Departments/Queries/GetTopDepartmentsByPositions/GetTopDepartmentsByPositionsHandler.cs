using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Dtos;
using DirectoryService.Contracts.Departments.Responses;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Queries.GetTopDepartmentsByPositions;

public class GetTopDepartmentsByPositionsHandler(
    IValidator<GetTopDepartmentsByPositionQuery> validator,
    IDbConnectionFactory connectionFactory) :
    IQueryHandler<GetTopDepartmentsByPositionsResponse, GetTopDepartmentsByPositionQuery>
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
                JOIN department_positions dp ON dp.department_id = d.id
                WHERE d.is_active = true
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