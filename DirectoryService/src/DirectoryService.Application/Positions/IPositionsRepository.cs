using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using Shared.Failures;

namespace DirectoryService.Application.Positions;

public interface IPositionsRepository
{
    Task<Result<PositionId, Error>> AddAsync(Position position, CancellationToken cancellationToken);

    Task<Result<Position, Error>> GetByAsync(Expression<Func<Position, bool>> expression, CancellationToken cancellationToken);

    Task<Result<bool, Error>> IsNameUniqueAsync(PositionName positionName, CancellationToken cancellationToken);
}