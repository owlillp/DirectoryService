using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILogger<CreateLocationHandler> _logger;
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ILocationsRepository _locationsRepository;

    public CreateLocationHandler(
        ILogger<CreateLocationHandler> logger,
        IValidator<CreateLocationCommand> validator,
        ILocationsRepository locationsRepository)
    {
        _logger = logger;
        _validator = validator;
        _locationsRepository = locationsRepository;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var request = command.Request;

        var locationNameResult = LocationName.Create(request.Name);

        var locationAddressResult = LocationAddress.Create(
            request.Address.Country,
            request.Address.City,
            request.Address.Street,
            request.Address.PostalCode,
            request.Address.BuildingNumber,
            request.Address.Apartment);

        var locationTimeZoneResult = LocationTimezone.Create(request.TimeZone);

        var location = Location.Create(
            locationNameResult.Value,
            locationAddressResult.Value,
            locationTimeZoneResult.Value);

        var addResult = await _locationsRepository.AddAsync(location, cancellationToken);
        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        _logger.LogInformation("Success created location with id: {locationId}", addResult.Value.Value);

        return location.Id.Value;
    }
}