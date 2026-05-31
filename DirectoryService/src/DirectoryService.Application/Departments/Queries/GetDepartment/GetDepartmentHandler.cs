using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Dtos;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.Failures;

namespace DirectoryService.Application.Departments.Queries.GetDepartment;

public class GetDepartmentHandler(
    IValidator<GetDepartmentQuery> validator,
    IReadDbContext dbContext)
    : IQueryHandler<DepartmentDto, GetDepartmentQuery>
{
    public async Task<Result<DepartmentDto, Errors>> Handle(GetDepartmentQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var departmentId = new DepartmentId(query.DepartmentId);

        var department = await dbContext
            .DepartmentsRead
            .Include(d => d.Positions)
            .Include(d => d.Locations)
            .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

        if (department == null)
        {
            return GeneralErrors.NotFound(nameof(Department), departmentId.Value).ToErrors();
        }

        return new DepartmentDto
        {
            Id = department.Id.Value,
            Name = department.Name.Value,
            Identifier = department.Identifier.Value,
            Path = department.Path.Value,
            ParentId = department.ParentId?.Value,
            Depth = department.Depth,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt,
            LocationIds = department.Locations.Select(l => l.LocationId.Value).ToList(),
            PositionIds = department.Positions.Select(l => l.PositionId.Value).ToList(),
        };
    }
}