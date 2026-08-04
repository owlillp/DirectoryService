namespace FileService.Contracts.Files.Dtos;

public record GetMediaAssetsDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string AssetType { get; init; } = null!;
    public string? Url { get; init; }
}