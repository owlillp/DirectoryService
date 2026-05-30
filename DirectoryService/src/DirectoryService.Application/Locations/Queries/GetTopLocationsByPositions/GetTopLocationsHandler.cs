using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations.Dtos;
using DirectoryService.Contracts.Locations.Responses;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Locations.Queries.GetTopLocationsByPositions;

public class GetTopLocationsHandler(
    IValidator<GetTopLocationsQuery> validator,
    IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetTopLocationsResponse, GetTopLocationsQuery>
{
    public async Task<Result<GetTopLocationsResponse, Errors>> Handle(
        GetTopLocationsQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);

        var topLocationDtos =
            (await connection.QueryAsync<TopLocationDto, int, TopLocationDto>(
                $"""
                SELECT 
                    l.id,
                    l.name,
                    l.country,
                    l.city,
                    l.street,
                    l.postal_code,
                    l.building_number,
                    l.apartment,
                    COUNT(dl.location_id)::int AS departments_count
                FROM locations l
                LEFT JOIN department_locations dl ON dl.location_id = l.id
                JOIN departments d ON dl.department_id = d.id
                WHERE l.is_active = TRUE
                    AND d.is_active = TRUE
                GROUP BY l.id, l.name
                ORDER BY departments_count DESC, l.name ASC
                LIMIT {query.TopCount}
                """,
                splitOn: "departments_count",
                map : (topLocationDto, departmentsCount)
                    => topLocationDto with { DepartmentsCount = departmentsCount }))
            .ToList();

        return new GetTopLocationsResponse(topLocationDtos, topLocationDtos.Count);
    }
}