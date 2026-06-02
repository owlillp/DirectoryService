using System.Data;
using Core.Abstractions;
using Core.Abstractions.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Departments.Dtos;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel.Failures;

namespace DirectoryService.Application.Departments.Queries.GetChildDepartments;

public class GetChildDepartmentsHandler(
    IValidator<GetChildDepartmentsQuery> validator,
    IReadDbContext readDbContext,
    IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetChildDepartmentsResponse, GetChildDepartmentsQuery>
{
    private const string CHILD_LIMIT_PARAMETER = "child_limit";
    private const string CHILD_OFFSET_PARAMETER = "child_offset";
    private const string ROOT_ID_PARAMETER = "root_id";

    public async Task<Result<GetChildDepartmentsResponse, Errors>> Handle(
        GetChildDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var request = query.Request;

        var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);

        var parentId = new DepartmentId(query.ParentId);
        bool existParent = await readDbContext
            .DepartmentsRead
            .AnyAsync(d => d.Id == parentId, cancellationToken);

        if (!existParent)
        {
            return GeneralErrors.NotFound(nameof(Department), parentId.Value).ToErrors();
        }

        int offset = (request.Page - 1) * request.PageSize;

        var parameters = new DynamicParameters();
        parameters.Add(CHILD_LIMIT_PARAMETER, request.PageSize, DbType.Int32);
        parameters.Add(CHILD_OFFSET_PARAMETER, offset, DbType.Int32);
        parameters.Add(ROOT_ID_PARAMETER, query.ParentId, DbType.Guid);

        long? childCount = null;

        var departmentDtoList = (await connection.QueryAsync<DepartmentWithChildrenDto, long, DepartmentWithChildrenDto>(
            $"""
             SELECT  d.id,
                     d.name,
                     d.identifier,
                     d.parent_id,
                     d.path,
                     d.depth,
                     d.is_active,
                     d.created_at,
                     d.updated_at,
                     EXISTS(SELECT 1 FROM departments c WHERE c.parent_id = d.Id AND c.is_active = true) AS has_more_children,
                     COUNT(*) OVER () AS child_count
             FROM departments d
             WHERE d.parent_id = @{ROOT_ID_PARAMETER} AND d.is_active = TRUE
             ORDER BY d.created_at, d.name
             OFFSET @{CHILD_OFFSET_PARAMETER}
             LIMIT @{CHILD_LIMIT_PARAMETER}
             """,
            param: parameters,
            splitOn: "child_count",
            map: (departmentDto, totalChildCount) =>
            {
                childCount ??= totalChildCount;
                return departmentDto;
            })).ToList();

        return new GetChildDepartmentsResponse(
            query.ParentId,
            new PagedResult<DepartmentWithChildrenDto>(departmentDtoList, childCount ?? 0));
    }
}