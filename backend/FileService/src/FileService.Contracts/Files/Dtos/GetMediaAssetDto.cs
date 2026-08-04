namespace FileService.Contracts.Files.Dtos;

public record GetMediaAssetDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string AssetType { get; init; } = null!;
    public string Context { get; init; } = null!;
    public long Size { get; init; }
    public Guid ContextId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string? Url { get; init; }
}