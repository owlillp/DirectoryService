namespace FileService.VideoProcessing.Preview;

public interface IPreviewGenerator
{
    IReadOnlyList<TimeSpan> CalculateExtractionTimes(TimeSpan duration);
}