using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions.Commands.CreatePosition;
using DirectoryService.Contracts.Positions;
using DirectoryService.Contracts.Positions.Requests;
using Microsoft.AspNetCore.Mvc;
using Shared.EndpointResults;

namespace DirectoryService.Presentation.Positions;

[ApiController]
[Route("/api/[controller]")]
public class PositionsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(request);
        return await handler.Handle(command, cancellationToken);
    }
}