using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Failures;

namespace DirectoryService.Infrastructure.Postgres.Locations;

public class LocationsRepository : ILocationsRepository
{
    private readonly ILogger<LocationsRepository> _logger;
    private readonly DirectoryServiceDbContext _dbContext;

    public LocationsRepository(ILogger<LocationsRepository> logger, DirectoryServiceDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Result<LocationId, Error>> AddAsync(Location location, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Add(location);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return location.Id;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null })
            {
                if (pgEx.ConstraintName.Contains("ix_locations_name", StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogError(ex, "Database name conflict error while creating location with name: {locationName}", location.Name.Value);
                    return GeneralErrors.Conflict(nameof(Location), nameof(location.Name));
                }

                if (pgEx.ConstraintName.Contains("ix_locations_address", StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogError(ex, "Database address conflict error while creating location with name: {locationName}", location.Name.Value);
                    return GeneralErrors.Conflict(nameof(Location), nameof(location.Name));
                }

                _logger.LogError(ex, "Database update error while creating location with name: {Name}", location.Name.Value);
                return GeneralErrors.Failure();
            }

            _logger.LogError(ex, "Database update error while creating location with name: {locationName}", location.Name.Value);
            return GeneralErrors.Failure();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation was canceled while creating location with name: {locationName}", location.Name.Value);
            return GeneralErrors.Canceled("Create Location");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating location with name: {name}", location.Name.Value);
            return GeneralErrors.Failure();
        }
    }
}