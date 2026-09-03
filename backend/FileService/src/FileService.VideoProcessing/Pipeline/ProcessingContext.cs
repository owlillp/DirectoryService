using CSharpFunctionalExtensions;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Domain.MediaProcessing;
using Shared.SharedKernel.Failures;

namespace FileService.VideoProcessing.Pipeline;

public record ProcessingContext
{
    private readonly List<StorageKey> _previewKeys = new();

    private const string HLS_SUBDIRECTORY = "hls";

    public required VideoProcess VideoProcess { get; init; }

    public required VideoAsset VideoAsset { get; init; }

    public string? WorkingDirectory { get; private set; }

    public string? HlsOutputDirectory { get; private set; }

    public string? MediaAssetUrl { get; private set; }

    public StorageKey? SpritePreviewKey { get; private set; }

    public IReadOnlyList<StorageKey> PreviewKeys => _previewKeys.AsReadOnly();

    public UnitResult<Error> CreateWorkingDirectory()
    {
        try
        {
            WorkingDirectory = Directory.CreateTempSubdirectory("video-processing").FullName;

            HlsOutputDirectory = Path.Combine(WorkingDirectory, HLS_SUBDIRECTORY);
            Directory.CreateDirectory(HlsOutputDirectory);
        }
        catch (Exception ex)
        {
            return Error.Failure("working.directory.creation", $"Failed to create working directory: {ex.Message}");
        }

        return UnitResult.Success<Error>();
    }

    public void SetPreviewKeys(IEnumerable<StorageKey> previewKeys, StorageKey? spriteKey = null)
    {
        _previewKeys.Clear();
        _previewKeys.AddRange(previewKeys);
        SpritePreviewKey = spriteKey;
    }

    public void SetMediaAssetUrl(string mediaAssetUrl) => MediaAssetUrl = mediaAssetUrl;

    internal void Cleanup()
    {
        WorkingDirectory = null;
        HlsOutputDirectory = null;
        MediaAssetUrl = null;
    }
}