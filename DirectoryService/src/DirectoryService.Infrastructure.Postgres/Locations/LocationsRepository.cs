using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared.Errors;
using Microsoft.Extensions.Logging;

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

    public async Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Add(location);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return location.Id;
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to add location with error: {error}", e.Message);
            return Result.Failure<Guid, Error>(Error.Table("location.add.error", e.Message));
        }
    }
}