using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using Shared.Failures;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Result<LocationId, Error>> AddAsync(Location location, CancellationToken cancellationToken);

    Task<Result<Location, Error>> GetByAsync(Expression<Func<Location, bool>> expression, CancellationToken cancellationToken);

    Task<Result<bool, Error>> ExistAsync(IEnumerable<LocationId> locationIds, CancellationToken cancellationToken);
}