using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Locations;

[ApiController]
[Route("api/locations")]
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
}