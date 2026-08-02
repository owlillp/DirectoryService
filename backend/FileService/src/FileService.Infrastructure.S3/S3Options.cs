namespace FileService.Infrastructure.S3;

public record S3Options
{
    public string Endpoint { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public bool WithSsl { get; init; }
    public int DownloadExpirationHours { get; init; } = 24;
    public int UploadExpirationMinutes { get; init; } = 60;
    public int MaxConcurrentRequests { get; init; } = 20;
    public IReadOnlyList<string> RequiredBuckets { get; init; } = [];
}