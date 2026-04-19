using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILogger<CreateLocationHandler> _logger;
    private readonly ILocationsRepository _locationsRepository;

    public CreateLocationHandler(
        ILogger<CreateLocationHandler> logger,
        ILocationsRepository locationsRepository)
    {
        _logger = logger;
        _locationsRepository = locationsRepository;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Dto;

        var locationNameResult = LocationName.Create(dto.Name);
        if (locationNameResult.IsFailure)
            return Result.Failure<Guid, Errors>(locationNameResult.Error);

        var locationAddressResult = LocationAddress.Create(
            dto.Country,
            dto.City,
            dto.Street,
            dto.PostalCode,
            dto.BuildingNumber,
            dto.Apartment);
        if(locationAddressResult.IsFailure)
            return Result.Failure<Guid, Errors>(locationAddressResult.Error);

        var locationTimeZoneResult = LocationTimezone.Create(dto.TimeZone);
        if (locationTimeZoneResult.IsFailure)
            return Result.Failure<Guid, Errors>(locationTimeZoneResult.Error);

        var location = Location.Create(
            locationNameResult.Value,
            locationAddressResult.Value,
            locationTimeZoneResult.Value);

        var addResult = await _locationsRepository.AddAsync(location, cancellationToken);
        if (addResult.IsFailure)
            return Result.Failure<Guid, Errors>(addResult.Error);

        _logger.LogInformation("Success created location with id: {locationId}", addResult.Value);

        return location.Id.Value;
    }
}