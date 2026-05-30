using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Positions.Dtos;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.Failures;

namespace DirectoryService.Application.Positions.Queries.GetPosition;

public class GetPositionHandler(
    IValidator<GetPositionQuery> validator,
    IReadDbContext dbContext)
    : IQueryHandler<PositionDto, GetPositionQuery>
{
    public async Task<Result<PositionDto, Errors>> Handle(GetPositionQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var positionId = new PositionId(query.PositionId);

        var position = await dbContext
            .PositionsRead
            .Include(p => p.Departments)
            .FirstOrDefaultAsync(p => p.Id == positionId, cancellationToken);

        if (position == null)
        {
            return GeneralErrors.NotFound(nameof(Position), positionId.Value).ToErrors();
        }

        return new PositionDto
        {
            Id = position.Id.Value,
            Name = position.Name.Value,
            Description = position.Description?.Value,
            DepartmentIds = position.Departments.Select(dp => dp.DepartmentId.Value).ToList(),
            IsActive = position.IsActive,
            CreatedAt = position.CreatedAt,
        };
    }
}