using Microsoft.Extensions.Options;

namespace FileService.VideoProcessing.Preview;

public class PreviewGenerator(IOptions<PreviewOptions> options) : IPreviewGenerator
{
    private readonly PreviewOptions _options = options.Value;

    public IReadOnlyList<TimeSpan> CalculateExtractionTimes(TimeSpan duration)
    {
        int previewCount = Math.Max((int)(duration.TotalSeconds / _options.SecondsPerPreview), 1);
        previewCount = Math.Min(previewCount, _options.MaxPreviewCount);
        double interval = duration.TotalSeconds / previewCount;

        List<TimeSpan> result = new();
        for (int i = 0; i < previewCount; i++)
        {
            result.Add(TimeSpan.FromSeconds(interval * i));
        }

        return result;
    }
}