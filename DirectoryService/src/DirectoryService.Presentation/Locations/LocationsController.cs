using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Commands.CreateLocation;
using DirectoryService.Application.Locations.Queries.GetLocations;
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

    [HttpGet]
    public async Task<EndpointResult<GetLocationsResponse>> Get(
        [FromServices] IQueryHandler<GetLocationsResponse, GetLocationsQuery> handler,
        [FromQuery] GetLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationsQuery(request);
        return await handler.Handle(query, cancellationToken);
    }
}