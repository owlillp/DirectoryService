using FileService.Domain.Assets;
using FileService.Domain.MediaProcessing;

namespace FileService.VideoProcessing.Pipeline;

public record ProcessingContext
{
    public required VideoProcess VideoProcess { get; init; }

    public required VideoAsset VideoAsset { get; init; }

    public string? WorkingDirectory { get; private set; }

    public string? HlsOutputDirectory { get; private set; }

    public string? MediaAssetUrl { get; private set; }
}