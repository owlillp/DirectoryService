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

namespace DirectoryService.Application.Departments.Queries.GetRootDepartments;

public class GetRootDepartmentsHandler(
    IValidator<GetRootDepartmentsQuery> validator,
    IDbConnectionFactory connectionFactory)
    : IQueryHandler<PagedResult<DepartmentWithChildrenDto>, GetRootDepartmentsQuery>
{
    private const string ROOT_LIMIT_PARAMETER = "root_limit";
    private const string ROOT_OFFSET_PARAMETER = "root_offset";
    private const string CHILD_LIMIT_PARAMETER = "child_limit";

    public async Task<Result<PagedResult<DepartmentWithChildrenDto>, Errors>> Handle(
        GetRootDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var request = query.Request;

        var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);

        int offset = (request.Page - 1) * request.Size;

        var parameters = new DynamicParameters();
        parameters.Add(ROOT_LIMIT_PARAMETER, request.Size, DbType.Int32);
        parameters.Add(ROOT_OFFSET_PARAMETER, offset, DbType.Int32);
        parameters.Add(CHILD_LIMIT_PARAMETER, request.Prefetch, DbType.Int32);

        long? rootsCount = null;
        Dictionary<Guid, DepartmentWithChildrenDto> departmentDtoMap = [];

        await connection.QueryAsync<DepartmentWithChildrenDto, long, DepartmentWithChildrenDto>(
            $"""
            WITH roots AS (
                            SELECT   r.id,
                                    r.name,
                                    r.identifier,
                                    r.parent_id,
                                    r.path,
                                    r.depth,
                                    r.is_active,
                                    r.created_at,
                                    r.updated_at
                            FROM departments r
                            WHERE r.parent_id IS NULL
                            ORDER BY r.created_at, r.name
                            OFFSET @{ROOT_OFFSET_PARAMETER}
                            LIMIT @{ROOT_LIMIT_PARAMETER}),
                all_roots AS (
                            SELECT COUNT(*) as count
                            FROM departments
                            WHERE parent_id IS NULL)
            SELECT roots.*,
                   (EXISTS (SELECT 1 FROM departments WHERE parent_id = roots.id OFFSET @{CHILD_LIMIT_PARAMETER})) AS has_more_children,
                   all_roots.count as roots_count
            FROM roots
            CROSS JOIN all_roots
            
            UNION ALL
            
            SELECT c.*,
                   (EXISTS (SELECT 1 FROM departments WHERE parent_id = c.id)) AS has_more_children,
                   all_roots.count as roots_count
            FROM roots r
            CROSS JOIN all_roots
            CROSS JOIN LATERAL (SELECT
                                    c.id,
                                    c.name,
                                    c.identifier,
                                    c.parent_id,
                                    c.path,
                                    c.depth,
                                    c.is_active,
                                    c.created_at,
                                    c.updated_at
                                FROM departments c
                                WHERE c.parent_id = r.id AND r.is_active = TRUE
                                ORDER BY c.created_at, c.name
                                LIMIT @{CHILD_LIMIT_PARAMETER}) c
            """,
            param: parameters,
            splitOn: "roots_count",
            map: (departmentDto, totalRootCount) =>
            {
                if (departmentDto.ParentId == null)
                {
                    departmentDtoMap.Add(departmentDto.Id, departmentDto);
                }
                else if(departmentDtoMap.TryGetValue(departmentDto.ParentId.Value, out var parent))
                {
                    parent.Children.Add(departmentDto);
                }

                rootsCount ??= totalRootCount;
                return departmentDto;
            });

        return new PagedResult<DepartmentWithChildrenDto>(departmentDtoMap.Values.ToList(), rootsCount ?? 0);
    }
}
