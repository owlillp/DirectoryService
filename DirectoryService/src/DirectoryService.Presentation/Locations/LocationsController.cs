using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Locations;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        [FromBody] CreateLocationDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(dto);
        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }
}