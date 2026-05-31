using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions.Commands.CreatePosition;
using DirectoryService.Application.Positions.Commands.SoftDelete;
using DirectoryService.Application.Positions.Commands.UpdatePosition;
using DirectoryService.Application.Positions.Queries.GetPosition;
using DirectoryService.Contracts.Positions.Dtos;
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

    [HttpPatch("{positionId:guid}")]
    public async Task<EndpointResult> Update(
        [FromServices] ICommandHandler<UpdatePositionCommand> handler,
        [FromRoute] Guid positionId,
        [FromBody] UpdatePositionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePositionCommand(positionId, request);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{positionId:guid}")]
    public async Task<EndpointResult> SoftDelete(
        [FromServices] ICommandHandler<SoftDeletePositionCommand> handler,
        [FromRoute] Guid positionId,
        CancellationToken cancellationToken)
    {
        var command = new SoftDeletePositionCommand(positionId);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet("{positionId:guid}")]
    public async Task<EndpointResult<PositionDto>> Get(
        [FromServices] IQueryHandler<PositionDto, GetPositionQuery> handler,
        [FromRoute] Guid positionId,
        CancellationToken cancellationToken)
    {
        var query = new GetPositionQuery(positionId);
        return await handler.Handle(query, cancellationToken);
    }
}