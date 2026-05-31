using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Dtos;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentAncestors;

public class GetDepartmentAncestorsHandler(
    IValidator<GetDepartmentAncestorsQuery> validator,
    IReadDbContext readDbContext,
    IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetDepartmentAncestorsResponse, GetDepartmentAncestorsQuery>
{
    private const string TARGET_DEPARTMENT_ID_PARAMETER = "target_department_id";

    public async Task<Result<GetDepartmentAncestorsResponse, Errors>> Handle(GetDepartmentAncestorsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var departmentId = new DepartmentId(query.TargetDepartmentId);

        bool exist = await readDbContext.DepartmentsRead.AnyAsync(d => d.Id == departmentId, cancellationToken);
        if (!exist)
        {
            return GeneralErrors.NotFound(nameof(Department), departmentId.Value).ToErrors();
        }

        var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add(TARGET_DEPARTMENT_ID_PARAMETER, query.TargetDepartmentId, DbType.Guid);

        var ancestorDtos = (await connection.QueryAsync<AncestorDepartmentDto>(
            $"""
             SELECT
                 d.id,
                 d.parent_id,
                 d.name,
                 d.identifier,
                 d.depth,
                 d.path
             FROM departments d
             WHERE d.path @> (SELECT path
                              FROM departments
                              WHERE id = @{TARGET_DEPARTMENT_ID_PARAMETER}::uuid)
                AND d.id != @{TARGET_DEPARTMENT_ID_PARAMETER}::uuid
                AND d.is_active = TRUE
             ORDER BY d.depth
             """,
            param: parameters))
            .ToList();

        return new GetDepartmentAncestorsResponse(query.TargetDepartmentId, ancestorDtos);
    }
}