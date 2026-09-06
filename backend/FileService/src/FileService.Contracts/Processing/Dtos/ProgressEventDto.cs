namespace FileService.Contracts.Processing.Dtos;

public record ProgressEventDto
{
    public Guid MediaAssetId { get; init; }
    public string ProcessStatus { get; init; } = string.Empty;
    public double Percent { get; init; }
    public int? StepOrder { get; init; }
    public string? StepName { get; init; }
    public int TotalSteps { get; init; }
    public string? Error { get; init; }
    public string? ErrorCode { get; init; }
    public DateTime PublishedAt { get; init; }
}