using Core.Abstractions;
using FileService.Application.Features.Commands.AbortMultipartUpload;
using FileService.Application.Features.Commands.AbortUpload;
using FileService.Application.Features.Commands.CompleteMultipartUpload;
using FileService.Application.Features.Commands.CompleteUpload;
using FileService.Application.Features.Commands.Delete;
using FileService.Application.Features.Commands.StartMultipartUpload;
using FileService.Application.Features.Commands.StartUpload;
using FileService.Application.Features.Queries.CheckFileExist;
using FileService.Application.Features.Queries.GetMediaAsset;
using FileService.Application.Features.Queries.GetMediaAssetsForEntity;
using FileService.Contracts.Files.Dtos;
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

    [HttpPost("{fileId:guid}/complete")]
    public async Task<EndpointResult> CompleteUpload(
        [FromRoute] Guid fileId,
        [FromServices] ICommandHandler<CompleteUploadCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CompleteUploadCommand(fileId);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPost("{fileId:guid}/abort")]
    public async Task<EndpointResult> AbortUpload(
        [FromRoute] Guid fileId,
        [FromServices] ICommandHandler<AbortUploadCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new AbortUploadCommand(fileId);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPost("multipart/start")]
    public async Task<EndpointResult<StartMultipartUploadResponse>> StartMultipartUpload(
        [FromBody] StartMultipartUploadRequest request,
        [FromServices] ICommandHandler<StartMultipartUploadResponse, StartMultipartUploadCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new StartMultipartUploadCommand(request);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPost("multipart/complete")]
    public async Task<EndpointResult<Guid>> CompleteMultipartUpload(
        [FromBody] CompleteMultipartUploadRequest request,
        [FromServices] ICommandHandler<Guid, CompleteMultipartUploadCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CompleteMultipartUploadCommand(request);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPost("multipart/abort")]
    public async Task<EndpointResult> AbortMultipartUpload(
        [FromBody] AbortMultipartUploadRequest request,
        [FromServices] ICommandHandler<AbortMultipartUploadCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new AbortMultipartUploadCommand(request);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{fileId:guid}")]
    public async Task<EndpointResult> Delete(
        [FromRoute] Guid fileId,
        [FromServices] ICommandHandler<DeleteMediaAssetCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new DeleteMediaAssetCommand(fileId);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet("{fileId:guid}")]
    public async Task<EndpointResult<GetMediaAssetDto>> GetMediaAsset(
        [FromRoute] Guid fileId,
        [FromServices] IQueryHandler<GetMediaAssetDto, GetMediaAssetQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetMediaAssetQuery(fileId);
        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("entity")]
    public async Task<EndpointResult<GetFilesForEntityResponse>> GetMediaAssetsForEntity(
        [FromQuery] GetFilesForEntityRequest request,
        [FromServices] IQueryHandler<GetFilesForEntityResponse, GetMediaAssetsForEntityQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetMediaAssetsForEntityQuery(request);
        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("{fileId:guid}/exist")]
    public async Task<EndpointResult<bool>> CheckFileExists(
        [FromQuery] Guid fileId,
        [FromServices] IQueryHandler<bool, CheckFileExistQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new CheckFileExistQuery(fileId);
        return await handler.Handle(query, cancellationToken);
    }
}