namespace FileService.Application.Models;

public record FileStorageOptions
{
    public string Endpoint { get; init; } = string.Empty;
    public string ExternalEndpoint { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public bool WithSsl { get; init; }
    public int DownloadExpirationHours { get; init; } = 24;
    public int UploadExpirationMinutes { get; init; } = 60;
    public int MaxConcurrentRequests { get; init; } = 20;
    public int RecommendedChunkSizeBytes { get; init; } = 10 * 1024 * 1024;
    public int MaxChunks { get; init; } = 10000;
    public IReadOnlyList<string> RequiredBuckets { get; init; } = [];
}