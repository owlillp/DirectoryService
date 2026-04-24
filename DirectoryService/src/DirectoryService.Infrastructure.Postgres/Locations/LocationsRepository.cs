using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Failures;

namespace DirectoryService.Infrastructure.Postgres.Locations;

public class LocationsRepository(ILogger<LocationsRepository> logger, DirectoryServiceDbContext dbContext)
    : ILocationsRepository
{
    public async Task<Result<LocationId, Error>> AddAsync(Location location, CancellationToken cancellationToken)
    {
        try
        {
            dbContext.Locations.Add(location);

            await dbContext.SaveChangesAsync(cancellationToken);

            return location.Id;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null })
            {
                if (pgEx.ConstraintName.Contains("ix_locations_name", StringComparison.InvariantCultureIgnoreCase))
                {
                    logger.LogError(ex, "Database name conflict error while creating location with name: {name}", location.Name.Value);
                    return GeneralErrors.Conflict(nameof(Location), nameof(location.Name));
                }

                if (pgEx.ConstraintName.Contains("ix_locations_address", StringComparison.InvariantCultureIgnoreCase))
                {
                    logger.LogError(ex, "Database address conflict error while creating location with name: {name}", location.Name.Value);
                    return GeneralErrors.Conflict(nameof(Location), nameof(location.Address));
                }
            }

            logger.LogError(ex, "Database update error while creating location with name: {name}", location.Name.Value);
            return GeneralErrors.Failure();
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was canceled while creating location with name: {name}", location.Name.Value);
            return GeneralErrors.Canceled("Process create location");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating location with name: {name}", location.Name.Value);
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<bool, Error>> ExistAsync(IEnumerable<LocationId> locationIds, CancellationToken cancellationToken)
    {
        try
        {
            int existCount = await dbContext.Locations
                .CountAsync(l => locationIds.Contains(l.Id), cancellationToken);

            return existCount == locationIds.Count();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while check of exist location");
            return GeneralErrors.Failure();
        }
    }
}