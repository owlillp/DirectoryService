using FileService.Application.Abstractions.Processing;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Presentation.Processing;

[ApiController]
[Route("/processing")]
public class ProcessingController : ControllerBase
{
    [HttpGet("progress/{videoAssetId:guid}/realtime")]
    public IResult GetProgress(
        [FromRoute] Guid videoAssetId,
        [FromServices] IProgressStreamService streamService,
        CancellationToken cancellationToken)
    {
        var sseItems = streamService.StreamProgressAsync(videoAssetId, cancellationToken);
        return TypedResults.ServerSentEvents(sseItems, "progress");
    }
}