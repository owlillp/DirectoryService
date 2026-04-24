using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler(
    ILogger<CreateLocationHandler> logger,
    IValidator<CreateLocationCommand> validator,
    ILocationsRepository locationsRepository)
    : ICommandHandler<Guid, CreateLocationCommand>
{
    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var request = command.Request;

        var locationName = LocationName.Create(request.Name).Value;

        var locationAddress = LocationAddress.Create(
            request.Address.Country,
            request.Address.City,
            request.Address.Street,
            request.Address.PostalCode,
            request.Address.BuildingNumber,
            request.Address.Apartment)
            .Value;

        var locationTimeZone = LocationTimezone.Create(request.TimeZone).Value;

        var location = Location.Create(
            locationName,
            locationAddress,
            locationTimeZone);

        var addResult = await locationsRepository.AddAsync(location, cancellationToken);
        if (addResult.IsFailure)
        {
            return addResult.Error.ToErrors();
        }

        logger.LogInformation("Success created location with id: {locationId}", addResult.Value.Value);

        return addResult.Value.Value;
    }
}