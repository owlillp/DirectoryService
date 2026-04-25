using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace DirectoryService.Infrastructure.Postgres.Positions;

public class PositionsRepository(ILogger<PositionsRepository> logger, DirectoryServiceDbContext dbContext)
    : IPositionsRepository
{
    public async Task<Result<PositionId, Error>> AddAsync(Position position, CancellationToken cancellationToken)
    {
        try
        {
            dbContext.Positions.Add(position);

            await dbContext.SaveChangesAsync(cancellationToken);

            return position.Id;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was canceled while creating position with name: {name}", position.Name.Value);
            return GeneralErrors.Canceled("Process create position");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating position with name: {name}", position.Name.Value);
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<Position, Error>> GetByAsync(Expression<Func<Position, bool>> expression, CancellationToken cancellationToken)
    {
        try
        {
            var position = await dbContext.Positions
                .FirstOrDefaultAsync(expression, cancellationToken);

            return position != null
                ? position
                : GeneralErrors.NotFound(nameof(Position));
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was cancelled while getting position");
            return GeneralErrors.Canceled("Process get position");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while getting position");
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<bool, Error>> IsNameUniqueAsync(PositionName positionName, CancellationToken cancellationToken)
    {
        try
        {
            bool exists = await dbContext.Positions
                .Where(p => p.IsActive)
                .Select(p => p.Name)
                .ContainsAsync(positionName, cancellationToken);

            return !exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while check of unique position name: {name}", positionName.Value);
            return GeneralErrors.Failure();
        }
    }
}