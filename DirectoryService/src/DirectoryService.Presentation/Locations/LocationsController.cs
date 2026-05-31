using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Commands.CreateLocation;
using DirectoryService.Application.Locations.Commands.SoftDelete;
using DirectoryService.Application.Locations.Commands.UpdateLocation;
using DirectoryService.Application.Locations.Queries.GetLocation;
using DirectoryService.Application.Locations.Queries.GetLocations;
using DirectoryService.Application.Locations.Queries.GetTopLocationsByPositions;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Locations.Dtos;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Contracts.Locations.Responses;
using Microsoft.AspNetCore.Mvc;
using Shared.EndpointResults;

namespace DirectoryService.Presentation.Locations;

[ApiController]
[Route("/api/[controller]")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(request);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPatch("{locationId:guid}")]
    public async Task<EndpointResult> Update(
        [FromServices] ICommandHandler<UpdateLocationCommand> handler,
        [FromRoute] Guid locationId,
        [FromBody] UpdateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLocationCommand(locationId, request);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{locationId:guid}")]
    public async Task<EndpointResult> SoftDelete(
        [FromServices] ICommandHandler<SoftDeleteLocationCommand> handler,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken)
    {
        var command = new SoftDeleteLocationCommand(locationId);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet("{locationId:guid}")]
    public async Task<EndpointResult<LocationDto>> Get(
        [FromServices] IQueryHandler<LocationDto, GetLocationQuery> handler,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationQuery(locationId);
        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet]
    public async Task<EndpointResult<PagedResult<LocationDto>>> Get(
        [FromServices] IQueryHandler<PagedResult<LocationDto>, GetLocationsQuery> handler,
        [FromQuery] GetLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationsQuery(request);
        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("top")]
    public async Task<EndpointResult<GetTopLocationsResponse>> GetTopByPositionsCount(
        [FromServices] IQueryHandler<GetTopLocationsResponse, GetTopLocationsQuery> handler,
        [FromQuery] int topCount,
        CancellationToken cancellationToken)
    {
        var query = new GetTopLocationsQuery(topCount);
        return await handler.Handle(query, cancellationToken);
    }
}