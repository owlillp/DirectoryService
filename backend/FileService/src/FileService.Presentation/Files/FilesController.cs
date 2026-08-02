using Core.Abstractions;
using FileService.Application.Features.Commands.StartUpload;
using FileService.Contracts.Files.Requests;
using FileService.Contracts.Files.Responses;
using Framework.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Presentation.Files;

[ApiController]
[Route("/files")]
public class FilesController : ControllerBase
{
    [HttpPost("upload")]
    public async Task<EndpointResult<StartUploadResponse>> StartUpload(
        [FromBody] StartUploadRequest request,
        [FromServices] ICommandHandler<StartUploadResponse, StartUploadCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new StartUploadCommand(request);
        return await handler.Handle(command, cancellationToken);
    }

    /*[HttpPut("{fileId:guid}")]
    public async Task<EndpointResult<string>> GetFile(
        [FromRoute] Guid fileId,
        CancellationToken cancellationToken)
    {
        return UnitResult.Success<Errors>();
    }

    [HttpPost("complete/{fileId:guid}")]
    public async Task<EndpointResult> CompleteUpload(
        [FromRoute] Guid fileId,
        CancellationToken cancellationToken)
    {
        return UnitResult.Success<Errors>();
    }*/
}